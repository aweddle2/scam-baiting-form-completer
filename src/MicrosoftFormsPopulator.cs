using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class MicrosoftFormsPopulator : FormPopulatorBase
{
    private static readonly Dictionary<string, string> ChoiceAnswers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["What type of work schedule are you interested in ?"] = "Part time",
    };

    public MicrosoftFormsPopulator(IWebDriver driver, WebDriverWait wait)
        : base(driver, wait) { }

    protected override bool RunIteration(string url)
    {
        var randomName = FormData.Names[Random.Shared.Next(FormData.Names.Length)];
        var nameParts = randomName.ToLowerInvariant().Split(' ');
        var randomEmail = $"{string.Concat(nameParts)}{Random.Shared.Next(1950, 2006)}@{FormData.EmailProviders[Random.Shared.Next(FormData.EmailProviders.Length)]}";
        var randomCompany = FormData.Ftse100Companies[Random.Shared.Next(FormData.Ftse100Companies.Length)];
        var randomGender = FormData.GenderIdentities[Random.Shared.Next(FormData.GenderIdentities.Length)];
        var randomPhone = $"({Random.Shared.Next(200, 1000)}) {Random.Shared.Next(200, 1000)}-{Random.Shared.Next(0, 10000):D4}";
        var randomJob = FormData.JobTitles[Random.Shared.Next(FormData.JobTitles.Length)];
        var randomAge = Random.Shared.Next(18, 66).ToString();
        var randomNationality = FormData.Countries[Random.Shared.Next(FormData.Countries.Length)];

        var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = randomName,
            ["Email"] = randomEmail,
            ["Company"] = randomCompany,
            ["Gender"] = randomGender,
            ["Phone"] = randomPhone,
            ["Current Job"] = randomJob,
            ["Age"] = randomAge,
            ["Nationality"] = randomNationality,
        };

        _driver.Navigate().GoToUrl(url);
        WaitForFormLoad();

        if (IsFormClosed())
        {
            Console.WriteLine("  Form is closed — stopping this URL.");
            return false;
        }

        Console.WriteLine($"  Name: {randomName}  |  Email: {randomEmail}  |  Company: {randomCompany}");

        FillForm(answers, ChoiceAnswers);
        if (SubmitForm())
        {
            Console.WriteLine("  Maximum responses reached — stopping this URL.");
            return false;
        }

        return true;
    }

    private bool IsFormClosed()
    {
        var js = (IJavaScriptExecutor)_driver;
        return _driver.FindElements(By.CssSelector("[data-automation-id='errorTitle']"))
            .Any(e => (js.ExecuteScript("return arguments[0].textContent", e) as string ?? "")
                .Contains("This form is closed", StringComparison.OrdinalIgnoreCase));
    }

    private void FillForm(
        Dictionary<string, string> answers,
        Dictionary<string, string> choiceAnswers)
    {
        Console.WriteLine("  Scanning questions…");
        var questionSelectors = new[]
        {
            "div[data-automation-id*='questionItem']",
            "div[class*='office-form-question']",
            "div[class*='question-content']",
        };

        var questionContainers = new List<IWebElement>();
        foreach (var selector in questionSelectors)
        {
            var found = _driver.FindElements(By.CssSelector(selector));
            if (found.Count > 0) { questionContainers.AddRange(found); break; }
        }

        if (questionContainers.Count == 0)
        {
            Console.WriteLine("  Standard question containers not found — falling back to aria-labelledby inputs.");
            var inputs = _driver.FindElements(By.CssSelector("input[aria-labelledby], textarea[aria-labelledby]"));
            Console.WriteLine($"  Found {inputs.Count} labelled input(s).");
            FillInputs(inputs, answers);
        }
        else
        {
            Console.WriteLine($"  Found {questionContainers.Count} question(s).");
            foreach (var container in questionContainers)
                ProcessQuestion(container, answers, choiceAnswers);
        }
    }

    // Returns true if the form is closed (max responses reached).
    private bool SubmitForm()
    {
        Console.WriteLine("  Submitting…");
        var submitSelectors = new[]
        {
            "button[data-automation-id*='submit']",
            "button[class*='submit']",
            "button[type='submit']",
            "button[aria-label*='Submit']",
            "button[aria-label*='submit']",
        };

        IWebElement? submitBtn = null;
        foreach (var sel in submitSelectors)
        {
            submitBtn = _driver.FindElements(By.CssSelector(sel)).FirstOrDefault(e => e.Displayed);
            if (submitBtn != null) break;
        }

        if (submitBtn == null)
        {
            Console.WriteLine("  Submit button not found. Please submit the form manually in the browser.");
            return false;
        }

        submitBtn.Click();
        Thread.Sleep(2000);

        var errorEl = _driver.FindElements(By.CssSelector("[data-automation-id='submitError']"))
            .FirstOrDefault(e => e.Displayed);
        if (errorEl != null && errorEl.Text.Contains("maximum number of responses", StringComparison.OrdinalIgnoreCase))
            return true;

        Console.WriteLine("  Done — check the browser for a confirmation message.");
        return false;
    }

    private void ProcessQuestion(
        IWebElement container,
        Dictionary<string, string> answers,
        Dictionary<string, string> choiceAnswers)
    {
        var label = GetLabel(container);
        Console.Write($"  Question: \"{label}\" → ");

        var textInputs = container.FindElements(By.CssSelector(
            "input[type='text'], input:not([type]), input[type='email'], input[type='number'], textarea"));

        if (textInputs.Count > 0)
        {
            var answer = MatchAnswer(label, answers) ?? "yes";
            textInputs[0].Clear();
            textInputs[0].SendKeys(answer);
            Console.WriteLine($"filled \"{answer}\"");
            return;
        }

        var radios = container.FindElements(By.CssSelector("input[type='radio']"));
        if (radios.Count > 0)
        {
            var desired = MatchAnswer(label, choiceAnswers) ?? "yes";
            IWebElement? fallback = null;
            foreach (var radio in radios)
            {
                var optionLabel = radio.FindElement(By.XPath("../..")).Text.Trim();
                fallback ??= radio;
                if (optionLabel.Contains(desired, StringComparison.OrdinalIgnoreCase))
                {
                    radio.Click();
                    Console.WriteLine($"selected \"{optionLabel}\"");
                    return;
                }
            }
            fallback?.Click();
            Console.WriteLine($"selected first option (no \"{desired}\" found)");
            return;
        }

        var checkboxes = container.FindElements(By.CssSelector("input[type='checkbox']"));
        if (checkboxes.Count > 0)
        {
            var desired = MatchAnswer(label, choiceAnswers) ?? "yes";
            IWebElement? fallback = null;
            foreach (var cb in checkboxes)
            {
                var cbLabel = cb.FindElement(By.XPath("..")).Text.Trim();
                fallback ??= cb;
                if (cbLabel.Contains(desired, StringComparison.OrdinalIgnoreCase))
                {
                    if (!cb.Selected) cb.Click();
                    Console.WriteLine($"checked \"{cbLabel}\"");
                    return;
                }
            }
            if (fallback != null && !fallback.Selected) fallback.Click();
            Console.WriteLine($"checked first option (no \"{desired}\" found)");
            return;
        }

        var dropdowns = container.FindElements(By.CssSelector("[role='combobox'], [role='listbox'], select"));
        if (dropdowns.Count > 0)
        {
            Console.WriteLine("dropdown — manual handling may be required");
            return;
        }

        Console.WriteLine("(no interactive input found)");
    }

    private void FillInputs(
        IReadOnlyCollection<IWebElement> inputs,
        Dictionary<string, string> answers)
    {
        foreach (var input in inputs)
        {
            var labelledBy = input.GetDomAttribute("aria-labelledby") ?? "";
            var labelText = ResolveAriaLabelledBy(labelledBy);
            Console.Write($"  Input aria-labelledby=\"{labelledBy}\" (label: \"{labelText}\") → ");
            var value = MatchAnswer(labelText, answers) ?? "yes";
            input.Clear();
            input.SendKeys(value);
            Console.WriteLine($"filled \"{value}\"");
        }
    }

    private string GetLabel(IWebElement container)
    {
        var input = container.FindElements(By.CssSelector("[aria-labelledby]")).FirstOrDefault();
        if (input != null)
        {
            var text = ResolveAriaLabelledBy(input.GetDomAttribute("aria-labelledby") ?? "");
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }
        return "(unlabelled)";
    }

    private string ResolveAriaLabelledBy(string labelledBy)
    {
        if (string.IsNullOrEmpty(labelledBy)) return "";
        var questionId = labelledBy.Split(' ')
            .FirstOrDefault(id => id.StartsWith("QuestionId_", StringComparison.Ordinal));
        if (questionId == null) return "";
        var labelEl = _driver.FindElements(By.Id(questionId)).FirstOrDefault();
        if (labelEl == null) return "";
        var js = (IJavaScriptExecutor)_driver;
        return ((js.ExecuteScript("return arguments[0].textContent", labelEl) as string) ?? "").Trim();
    }
}
