using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class JotFormPopulator : IFormPopulator
{
    public void Run(string url, int runCount)
    {
        var chromeOptions = new ChromeOptions();
        // chromeOptions.AddArgument("--headless=new");
        chromeOptions.AddArgument("--start-maximized");

        using var driver = new ChromeDriver(chromeOptions);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        for (int run = 1; run <= runCount; run++)
        {
            Console.WriteLine($"\n=== Run {run}/{runCount} ===");

            driver.Navigate().GoToUrl(url);
            WaitForForm(driver, wait);

            if (IsOverQuota(driver))
            {
                break;
            }

            var randomName = FormData.Names[Random.Shared.Next(FormData.Names.Length)];
            var joinedName = new string(string.Concat(randomName.ToLowerInvariant().Split(' '))
                .Where(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or '+').ToArray());
            var randomEmail = $"{joinedName}{Random.Shared.Next(1950, 2006)}@{FormData.EmailProviders[Random.Shared.Next(FormData.EmailProviders.Length)]}";
            var randomPhone = $"+1{Random.Shared.Next(200, 1000)}{Random.Shared.Next(200, 1000)}{Random.Shared.Next(0, 10000):D4}";
            var randomAreaCode = Random.Shared.Next(200, 1000).ToString();
            var randomPhoneNoAreaCode = Random.Shared.Next(200, 1000).ToString() + Random.Shared.Next(0, 10000).ToString("D4");
            var randomAge = Random.Shared.Next(18, 55).ToString();
            var randomNationality = FormData.Countries[Random.Shared.Next(FormData.Countries.Length)];
            var randomLocation = FormData.Cities[Random.Shared.Next(FormData.Cities.Length)];
            var telegramUsername = $"@{joinedName}{Random.Shared.Next(100, 999)}";

            Console.WriteLine($"  Name: {randomName}  |  Email: {randomEmail}  |  Location: {randomLocation}");

            var textAnswers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["name"] = randomName,
                ["age"] = randomAge,
                ["whatsapp"] = randomPhone,
                ["phone"] = randomPhone,
                ["phoneNumber"] = randomPhoneNoAreaCode,
                ["areaCode"] = randomAreaCode,
                ["email"] = randomEmail,
                ["telegram"] = telegramUsername,
                ["nationality"] = $"{randomNationality}. No prior experience, fresher.",
                ["experience"] = $"{randomNationality}. No prior experience, fresher.",
                ["location"] = randomLocation,
            };

            var radioAnswers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["interested"] = "Yes",
                ["previous"] = "No",
                ["applied"] = "No",
                ["similar"] = "No",
            };

            FillAndSubmit(driver, wait, textAnswers, radioAnswers);

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
        wait.Until(d =>
        {
            try
            {
                var candidates = d.FindElements(By.CssSelector(
                    "li.form-line[data-type], div.form-line[data-type], [data-type*='control_']," +
                    " input[id^='input_'], textarea[id^='input_'], h2[class='modal-heading-title']"));
                Console.WriteLine($"{candidates.Count} candidatse found.");
                return candidates.Count > 0;
            }
            catch (StaleElementReferenceException) { return false; }
        });
        Console.WriteLine("  Form loaded.");
    }

    private static bool IsOverQuota(IWebDriver driver)
    {
        var js = (IJavaScriptExecutor)driver;
        return driver.FindElements(By.CssSelector("h2.modal-heading-title"))
            .Any(e => (js.ExecuteScript("return arguments[0].textContent", e) as string ?? "")
                .Contains("Form over quota", StringComparison.OrdinalIgnoreCase));
    }

    private static void FillAndSubmit(
        IWebDriver driver, WebDriverWait wait,
        Dictionary<string, string> textAnswers,
        Dictionary<string, string> radioAnswers)
    {
        var js = (IJavaScriptExecutor)driver;
        int page = 1;

        while (true)
        {
            Console.WriteLine($"  --- Page {page} ---");
            FillVisibleFields(driver, js, textAnswers, radioAnswers);

            var nextBtn = FindVisible(driver,
                "button[class='jfWelcome-button']",
                "button[class*='jfInput-button forNext']");

            if (nextBtn != null)
            {
                Console.WriteLine("  → Advancing to next page…");
                js.ExecuteScript("arguments[0].click()", nextBtn);
                wait.Until(d => IsAnyFieldVisible(d));
                page++;
                continue;
            }

            var submitBtn = FindVisible(driver,
                "input[id='input_submit']", "input[id*='submit']",
                "button[id*='submit']", "input[type='submit']",
                "button[type='submit']", "button[class*='submit']",
                "input[class*='submit']");

            if (submitBtn != null)
            {
                Console.WriteLine("  → Submitting form…");
                js.ExecuteScript("arguments[0].click()", submitBtn);
                Thread.Sleep(2000);

                Console.WriteLine("  Submitted.");
                return;
            }

            Console.WriteLine("  No Next or Submit button found — please submit manually.");
            return;
        }
    }

    private static void FillVisibleFields(
        IWebDriver driver, IJavaScriptExecutor js,
        Dictionary<string, string> textAnswers,
        Dictionary<string, string> radioAnswers)
    {
        var lines = driver.FindElements(By.CssSelector("div[class='jfField']"))
            .Where(l => l.Displayed)
            .Distinct()
            .ToList();

        foreach (var line in lines)
        {
            var dataType = line.GetDomAttribute("data-type") ?? "";

            if (dataType == "")
                dataType = line.GetDomAttribute("type") ?? "";

            if (dataType is "control_head" or "control_text" or "control_image"
                or "control_pagebreak" or "control_widget" or "control_divider"
                or "control_collapse")
                continue;

            var label = GetFieldLabel(line);
            Console.Write($"  [{dataType}] \"{label}\" → ");

            switch (dataType)
            {
                case "textbox":
                case "email":
                case "areaCode":
                case "phone":
                    {
                        var val = MatchAnswer(label, textAnswers);
                        if (val != null)
                        {
                            var input = line.FindElements(By.TagName("input")).FirstOrDefault(e => e.Displayed && e.Enabled);
                            if (input != null) { input.Clear(); input.SendKeys(val); Console.WriteLine($"filled \"{val}\""); }
                            else Console.WriteLine("input not found");
                        }
                        else Console.WriteLine("no answer configured");
                        break;
                    }

                case "unknown":
                    {
                        var input = line.FindElements(By.CssSelector(
                                "input[type='tel'], input[type='text'], input:not([type])"))
                            .FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (input != null)
                        {
                            var val = MatchAnswer(label, textAnswers);
                            if (val != null) { input.Clear(); input.SendKeys(val); Console.WriteLine($"filled \"{val}\""); }
                            else Console.WriteLine("no answer configured");
                        }
                        else Console.WriteLine("input not found");
                        break;
                    }

                case "control_yesno":
                case "control_radio":
                    {
                        var desired = MatchAnswer(label, radioAnswers) ?? "Yes";
                        var radios = line.FindElements(By.CssSelector("input[type='radio']"));
                        bool clicked = false;
                        foreach (var radio in radios)
                        {
                            var optLabel = GetRadioOptionLabel(radio, driver);
                            if (optLabel.Contains(desired, StringComparison.OrdinalIgnoreCase))
                            {
                                js.ExecuteScript("arguments[0].click()", radio);
                                Console.WriteLine($"selected \"{optLabel}\"");
                                clicked = true;
                                break;
                            }
                        }
                        if (!clicked)
                            Console.WriteLine($"no match for \"{desired}\" among {radios.Count} option(s)");
                        break;
                    }

                case "control_checkbox":
                case "control_terms":
                    {
                        var checkboxes = line.FindElements(By.CssSelector("input[type='checkbox']"));
                        int checkedCount = 0;
                        foreach (var cb in checkboxes.Where(c => c.Displayed && !c.Selected))
                        {
                            js.ExecuteScript("arguments[0].click()", cb);
                            checkedCount++;
                        }
                        Console.WriteLine($"checked {checkedCount}/{checkboxes.Count}");
                        break;
                    }

                default:
                    Console.WriteLine($"(unhandled type: {dataType})");
                    break;
            }
        }
    }

    private static string GetFieldLabel(IWebElement line)
    {
        var label = line.FindElements(By.XPath("../../label")).FirstOrDefault();
        return label?.Text.Trim() ?? "(unlabelled)";
    }

    private static string GetRadioOptionLabel(IWebElement radio, IWebDriver driver)
    {
        try
        {
            var id = radio.GetDomAttribute("id") ?? "";
            if (!string.IsNullOrEmpty(id))
            {
                var lbl = driver.FindElements(By.CssSelector($"label[for='{id}']")).FirstOrDefault();
                if (lbl != null) return lbl.Text.Trim();
            }
            return radio.FindElement(By.XPath("following-sibling::label[1]")).Text.Trim();
        }
        catch { return radio.GetDomAttribute("value") ?? ""; }
    }

    private static bool IsAnyFieldVisible(IWebDriver driver)
    {
        try
        {
            return driver.FindElements(By.CssSelector(
                    "li.form-line[data-type], div.form-line[data-type], [data-type*='control_']"))
                .Any(l => l.Displayed);
        }
        catch (StaleElementReferenceException) { return false; }
    }

    private static IWebElement? FindVisible(IWebDriver driver, params string[] selectors)
    {
        foreach (var sel in selectors)
        {
            var el = driver.FindElements(By.CssSelector(sel))
                .FirstOrDefault(e => e.Displayed && e.Enabled);
            if (el != null) return el;
        }
        return null;
    }

    private static string? MatchAnswer(string label, Dictionary<string, string> answers)
    {
        foreach (var kv in answers)
            if (label.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        return "yes";
    }
}
