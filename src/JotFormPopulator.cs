using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class JotFormPopulator : FormPopulatorBase
{
    public JotFormPopulator(IWebDriver driver, WebDriverWait wait)
        : base(driver, wait) { }

    protected override bool RunIteration(string url, Dictionary<string, string> answers)
    {
        _driver.Navigate().GoToUrl(url);
        WaitForForm();

        if (IsOverQuota())
            return false;

        Console.WriteLine($"  Name: {answers["name"]}  |  Email: {answers["email"]}  |  Location: {answers["location"]}");

        FillAndSubmit(answers);
        return true;
    }

    private void WaitForForm()
    {
        _wait.Until(d =>
        {
            try
            {
                var candidates = d.FindElements(By.CssSelector(
                    "li.form-line[data-type], div.form-line[data-type], [data-type*='control_']," +
                    " input[id^='input_'], textarea[id^='input_'], h2[class='modal-heading-title']"));
                return candidates.Count > 0;
            }
            catch (StaleElementReferenceException) { return false; }
        });
        Console.WriteLine("  Form loaded.");
    }

    private bool IsOverQuota()
    {
        var js = (IJavaScriptExecutor)_driver;
        return _driver.FindElements(By.CssSelector("h2.modal-heading-title"))
            .Any(e => (js.ExecuteScript("return arguments[0].textContent", e) as string ?? "")
                .Contains("Form over quota", StringComparison.OrdinalIgnoreCase));
    }

    private void FillAndSubmit(Dictionary<string, string> answers)
    {
        var js = (IJavaScriptExecutor)_driver;
        int page = 1;

        while (true)
        {
            Console.WriteLine($"  --- Page {page} ---");
            FillVisibleFields(js, answers);

            var nextBtn = FindVisible(
                "button[class='jfWelcome-button']",
                "button[class*='jfInput-button forNext']");

            if (nextBtn != null)
            {
                Console.WriteLine("  → Advancing to next page…");
                js.ExecuteScript("arguments[0].click()", nextBtn);
                _wait.Until(d => IsAnyFieldVisible(d));
                page++;
                continue;
            }

            var submitBtn = FindVisible(
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

    private void FillVisibleFields(
        IJavaScriptExecutor js,
        Dictionary<string, string> answers)
    {
        var lines = _driver.FindElements(By.CssSelector("div[class='jfField']"))
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
                        var val = MatchAnswer(label, answers) ?? "yes";
                        var input = line.FindElements(By.TagName("input")).FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (input != null) { input.Clear(); input.SendKeys(val); Console.WriteLine($"filled \"{val}\""); }
                        else Console.WriteLine("input not found");
                        break;
                    }

                case "unknown":
                    {
                        var input = line.FindElements(By.CssSelector(
                                "input[type='tel'], input[type='text'], input:not([type])"))
                            .FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (input != null)
                        {
                            var val = MatchAnswer(label, answers) ?? "yes";
                            input.Clear(); input.SendKeys(val); Console.WriteLine($"filled \"{val}\"");
                        }
                        else Console.WriteLine("input not found");
                        break;
                    }

                case "control_yesno":
                case "control_radio":
                    {
                        var desired = MatchAnswer(label, answers) ?? "Yes";
                        var radios = line.FindElements(By.CssSelector("input[type='radio']"));
                        bool clicked = false;
                        foreach (var radio in radios)
                        {
                            var optLabel = GetRadioOptionLabel(radio);
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

    private string GetRadioOptionLabel(IWebElement radio)
    {
        try
        {
            var id = radio.GetDomAttribute("id") ?? "";
            if (!string.IsNullOrEmpty(id))
            {
                var lbl = _driver.FindElements(By.CssSelector($"label[for='{id}']")).FirstOrDefault();
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

    private IWebElement? FindVisible(params string[] selectors)
    {
        foreach (var sel in selectors)
        {
            var el = _driver.FindElements(By.CssSelector(sel))
                .FirstOrDefault(e => e.Displayed && e.Enabled);
            if (el != null) return el;
        }
        return null;
    }
}
