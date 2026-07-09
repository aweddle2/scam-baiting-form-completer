using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class FilloutFormPopulator : FormPopulatorBase
{
    private static readonly Dictionary<string, string> ChoiceAnswers =
        new(StringComparer.OrdinalIgnoreCase);

    public FilloutFormPopulator(IWebDriver driver, WebDriverWait wait)
        : base(driver, wait) { }

    protected override void WaitBetweenRuns()
    {
        var waitSeconds = Random.Shared.Next(1, 61);
        Console.WriteLine($"  Waiting {waitSeconds}s before next run…");
        Thread.Sleep(waitSeconds * 1000);
    }

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

        var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Please provide your full name"] = randomName,
            ["Provide your Email"] = randomEmail,
            ["Company"] = randomCompany,
            ["Gender"] = randomGender,
            ["Phone"] = randomPhone,
            ["Current Job"] = randomJob,
            ["Age"] = randomAge,
        };

        _driver.Navigate().GoToUrl(url);
        WaitForFormLoad();
        Thread.Sleep(2000);

        if (IsPageNotFound())
        {
            Console.WriteLine("  Page not found — stopping this URL.");
            return false;
        }

        Console.WriteLine($"  Name: {randomName}  |  Email: {randomEmail}  |  Company: {randomCompany}");

        Console.WriteLine("  Scanning questions…");
        var questionContainers = _driver.FindElements(By.CssSelector(
            "div[class='flex w-full relative sm:flex-row flex-col']")).ToList();

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
                ProcessQuestion(container, answers);
        }

        Console.WriteLine("  Submitting…");
        SubmitForm();

        return true;
    }

    private bool IsPageNotFound()
    {
        var js = (IJavaScriptExecutor)_driver;
        return _driver.FindElements(By.CssSelector("h1"))
            .Any(e => (js.ExecuteScript("return arguments[0].textContent", e) as string ?? "")
                .Contains("Page not found", StringComparison.OrdinalIgnoreCase));
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

    private static string GetLabel(IWebElement container)
    {
        var strong = container.FindElements(By.TagName("strong")).FirstOrDefault();
        return strong?.Text ?? "(unlabelled)";
    }

    private void ProcessQuestion(
        IWebElement container,
        Dictionary<string, string> answers)
    {
        var label = GetLabel(container);
        Console.Write($"Question: \"{label}\" → ");

        var textInputs = container.FindElements(By.CssSelector(
            "input[type='text'], input:not([type]), input[type='email'], input[type='number'], textarea"));

        if (textInputs.Count > 0)
        {
            var answer = MatchAnswer(label, answers);
            if (answer != null)
            {
                textInputs[0].Clear();
                textInputs[0].SendKeys(answer);
                Console.WriteLine($"filled \"{answer}\"");
            }
            else
            {
                Console.WriteLine("text input — no answer configured");
            }
            return;
        }

        var radios = container.FindElements(By.CssSelector("input[type='radio']"));
        if (radios.Count > 0)
        {
            var desired = MatchAnswer(label, ChoiceAnswers);
            if (desired != null)
            {
                foreach (var radio in radios)
                {
                    var optionLabel = radio.FindElement(By.XPath("..")).Text.Trim();
                    if (optionLabel.Contains(desired, StringComparison.OrdinalIgnoreCase))
                    {
                        radio.Click();
                        Console.WriteLine($"selected \"{optionLabel}\"");
                        return;
                    }
                }
            }
            Console.WriteLine($"{radios.Count} radio option(s) — no answer configured");
            return;
        }

        var checkboxes = container.FindElements(By.CssSelector("input[type='checkbox']"));
        if (checkboxes.Count > 0)
        {
            var desired = MatchAnswer(label, ChoiceAnswers);
            if (desired != null)
            {
                foreach (var cb in checkboxes)
                {
                    var cbLabel = cb.FindElement(By.XPath("..")).Text.Trim();
                    if (cbLabel.Contains(desired, StringComparison.OrdinalIgnoreCase) && !cb.Selected)
                    {
                        cb.Click();
                        Console.WriteLine($"checked \"{cbLabel}\"");
                        return;
                    }
                }
            }
            Console.WriteLine($"{checkboxes.Count} checkbox(es) — no answer configured");
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

    private void FillInputs(IReadOnlyCollection<IWebElement> inputs, Dictionary<string, string> answers)
    {
        foreach (var input in inputs)
        {
            var labelledBy = input.GetDomAttribute("aria-labelledby") ?? "";
            var labelText = ResolveAriaLabelledBy(labelledBy);
            Console.Write($"Input aria-labelledby=\"{labelledBy}\" (label: \"{labelText}\") → ");
            var answer = MatchAnswer(labelText, answers);
            if (answer != null)
            {
                input.Clear();
                input.SendKeys(answer);
                Console.WriteLine($"filled \"{answer}\"");
            }
            else
            {
                Console.WriteLine("no answer configured");
            }
        }
    }

    private void SubmitForm()
    {
        var submitBtn = _driver.FindElements(By.CssSelector("button[type='button']"))
            .FirstOrDefault(e => e.Displayed);

        if (submitBtn != null)
        {
            Console.WriteLine("Submitting form…");
            submitBtn.Click();
            Thread.Sleep(3000);
            Console.WriteLine("Done — check the browser for a confirmation message.");
        }
        else
        {
            Console.WriteLine("Submit button not found. Please submit the form manually in the browser.");
        }
    }
}
