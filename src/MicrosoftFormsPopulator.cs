using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class MicrosoftFormsPopulator : IFormPopulator
{
    public void Run(string url, int runCount)
    {
        var choiceAnswers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["What type of work schedule are you interested in ?"] = "Part time",
        };

        var chromeOptions = new ChromeOptions();
        // chromeOptions.AddArgument("--headless=new");
        chromeOptions.AddArgument("--start-maximized");

        using var driver = new ChromeDriver(chromeOptions);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        for (int run = 1; run <= runCount; run++)
        {
            Console.WriteLine($"\n=== Run {run}/{runCount} ===");

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

            Console.WriteLine($"  Name: {randomName}  |  Email: {randomEmail}  |  Company: {randomCompany}");

            driver.Navigate().GoToUrl(url);
            WaitForForm(driver, wait);
            FillForm(driver, answers, choiceAnswers);
            SubmitForm(driver);

            var waitSeconds = Random.Shared.Next(2, 12);
            Console.WriteLine($"  Waiting {waitSeconds}s before next run…");
            Thread.Sleep(waitSeconds * 1000);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("  [DEBUG] Press Enter to continue…");
                Console.ReadLine();
            }
        }

        Console.WriteLine($"\nAll {runCount} run(s) complete.");
    }

    private static void WaitForForm(IWebDriver driver, WebDriverWait wait)
    {
        Console.WriteLine("  Waiting for form to load…");
        wait.Until(d =>
        {
            try
            {
                var loadingElements = d.FindElements(By.CssSelector("[class*='loading'], [aria-label='Loading']"));
                return loadingElements.All(e => !e.Displayed);
            }
            catch (StaleElementReferenceException) { return false; }
        });
    }

    private static void FillForm(
        IWebDriver driver,
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
            var found = driver.FindElements(By.CssSelector(selector));
            if (found.Count > 0) { questionContainers.AddRange(found); break; }
        }

        if (questionContainers.Count == 0)
        {
            Console.WriteLine("  Standard question containers not found — falling back to aria-labelledby inputs.");
            var inputs = driver.FindElements(By.CssSelector("input[aria-labelledby], textarea[aria-labelledby]"));
            Console.WriteLine($"  Found {inputs.Count} labelled input(s).");
            FillInputs(inputs, answers, driver);
        }
        else
        {
            Console.WriteLine($"  Found {questionContainers.Count} question(s).");
            foreach (var container in questionContainers)
                ProcessQuestion(container, driver, answers, choiceAnswers);
        }
    }

    private static void SubmitForm(IWebDriver driver)
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
            submitBtn = driver.FindElements(By.CssSelector(sel)).FirstOrDefault(e => e.Displayed);
            if (submitBtn != null) break;
        }

        if (submitBtn != null)
        {
            submitBtn.Click();
            Console.WriteLine("  Done — check the browser for a confirmation message.");
        }
        else
        {
            Console.WriteLine("  Submit button not found. Please submit the form manually in the browser.");
        }
    }

    private static void ProcessQuestion(
        IWebElement container,
        IWebDriver driver,
        Dictionary<string, string> answers,
        Dictionary<string, string> choiceAnswers)
    {
        var label = GetLabel(container, driver);
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

    private static void FillInputs(
        IReadOnlyCollection<IWebElement> inputs,
        Dictionary<string, string> answers,
        IWebDriver driver)
    {
        foreach (var input in inputs)
        {
            var labelledBy = input.GetDomAttribute("aria-labelledby") ?? "";
            var labelText = ResolveAriaLabelledBy(labelledBy, driver);
            Console.Write($"  Input aria-labelledby=\"{labelledBy}\" (label: \"{labelText}\") → ");
            var answer = MatchAnswer(labelText, answers);
            var value = answer ?? "yes";
            input.Clear();
            input.SendKeys(value);
            Console.WriteLine($"filled \"{value}\"");
        }
    }

    private static string GetLabel(IWebElement container, IWebDriver driver)
    {
        var input = container.FindElements(By.CssSelector("[aria-labelledby]")).FirstOrDefault();
        if (input != null)
        {
            var text = ResolveAriaLabelledBy(input.GetDomAttribute("aria-labelledby") ?? "", driver);
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }
        return "(unlabelled)";
    }

    private static string ResolveAriaLabelledBy(string labelledBy, IWebDriver driver)
    {
        if (string.IsNullOrEmpty(labelledBy)) return "";
        var questionId = labelledBy.Split(' ')
            .FirstOrDefault(id => id.StartsWith("QuestionId_", StringComparison.Ordinal));
        if (questionId == null) return "";
        var labelEl = driver.FindElements(By.Id(questionId)).FirstOrDefault();
        if (labelEl == null) return "";
        var js = (IJavaScriptExecutor)driver;
        return ((js.ExecuteScript("return arguments[0].textContent", labelEl) as string) ?? "").Trim();
    }

    private static string? MatchAnswer(string label, Dictionary<string, string> answers)
    {
        foreach (var kv in answers)
            if (label.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        return null;
    }
}
