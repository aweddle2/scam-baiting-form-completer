using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ScamBaitingFormCompleter.FormPopulators;

public class GoogleFormsPopulator : FormPopulatorBase
{
    // Google Forms renders every question as a role='listitem' with a role='heading' label.
    // The selectors are public so the tests can build a fake DOM against the exact same strings.
    public const string QuestionSelector = "div[role='listitem']";
    public const string HeadingSelector = "div[role='heading']";
    public const string TextInputSelector =
        "input[type='text'], input[type='email'], input[type='tel'], input[type='number'], textarea";
    public const string RadioSelector = "div[role='radio']";
    public const string CheckboxSelector = "div[role='checkbox']";
    public const string ListboxSelector = "div[role='listbox']";
    public const string OptionSelector = "div[role='option']";
    public const string ButtonSelector = "div[role='button']";
    public const string ClosedHeadingSelector = "div[role='heading'], h1";

    public const string FieldSelector =
        TextInputSelector + ", " + RadioSelector + ", " + CheckboxSelector + ", " + ListboxSelector;

    // A form that keeps showing "Next" forever would otherwise spin here.
    private const int MaxPages = 25;

    public GoogleFormsPopulator(IWebDriver driver, WebDriverWait wait)
        : base(driver, wait) { }

    protected override bool RunIteration(string url, Dictionary<string, string> answers)
    {
        _driver.Navigate().GoToUrl(url);
        WaitForForm();

        if (IsFormClosed())
        {
            Console.WriteLine("  Form is no longer accepting responses — stopping this URL.");
            return false;
        }

        Console.WriteLine($"  Name: {answers["name"]}  |  Email: {answers["email"]}  |  Location: {answers["location"]}");

        FillAndSubmit(answers);
        return true;
    }

    // Google Forms ships the fields disabled and enables them once its JS hydrates,
    // so wait for something actually interactive rather than just present.
    private void WaitForForm()
    {
        try
        {
            _wait.Until(d => IsAnyFieldReady(d) || IsFormClosed());
            Console.WriteLine("  Form loaded.");
        }
        catch (WebDriverTimeoutException)
        {
            Console.WriteLine("  Timed out waiting for the form to become interactive — continuing anyway.");
        }
    }

    private static bool IsAnyFieldReady(IWebDriver driver)
    {
        try
        {
            return driver.FindElements(By.CssSelector(FieldSelector)).Any(e => e.Displayed && e.Enabled);
        }
        catch (StaleElementReferenceException) { return false; }
    }

    private bool IsFormClosed()
    {
        var js = (IJavaScriptExecutor)_driver;
        return _driver.FindElements(By.CssSelector(ClosedHeadingSelector))
            .Any(e => (js.ExecuteScript("return arguments[0].textContent", e) as string ?? "")
                .Contains("no longer accepting responses", StringComparison.OrdinalIgnoreCase));
    }

    private void FillAndSubmit(Dictionary<string, string> answers)
    {
        var js = (IJavaScriptExecutor)_driver;

        for (int page = 1; page <= MaxPages; page++)
        {
            Console.WriteLine($"  --- Page {page} ---");
            FillVisibleQuestions(js, answers);

            var nextBtn = FindButton("Next");
            if (nextBtn != null)
            {
                Console.WriteLine("  → Advancing to next page…");
                js.ExecuteScript("arguments[0].click()", nextBtn);
                try { _wait.Until(d => IsAnyFieldReady(d)); }
                catch (WebDriverTimeoutException) { Console.WriteLine("  Next page never became interactive."); }
                continue;
            }

            var submitBtn = FindButton("Submit");
            if (submitBtn != null)
            {
                Console.WriteLine("  → Submitting form…");
                js.ExecuteScript("arguments[0].click()", submitBtn);
                PauseAfterSubmit();
                Console.WriteLine("  Submitted.");
                return;
            }

            Console.WriteLine("  No Next or Submit button found — please submit manually.");
            return;
        }

        Console.WriteLine($"  Gave up after {MaxPages} pages.");
    }

    // Seam so the tests don't have to sit through the post-submit settle.
    protected virtual void PauseAfterSubmit() => Thread.Sleep(2000);

    private void FillVisibleQuestions(IJavaScriptExecutor js, Dictionary<string, string> answers)
    {
        var questions = _driver.FindElements(By.CssSelector(QuestionSelector))
            .Where(q => q.Displayed)
            .ToList();

        Console.WriteLine($"  Found {questions.Count} question(s).");

        foreach (var question in questions)
            FillQuestion(js, question, answers);
    }

    private void FillQuestion(IJavaScriptExecutor js, IWebElement question, Dictionary<string, string> answers)
    {
        var heading = GetHeadingText(question);
        var label = CleanLabel(heading);
        // Match on the question itself first, then fall back to the whole heading — some
        // questions bury the only useful keyword up in the preamble.
        var answer = MatchBestAnswer(label, answers) ?? MatchBestAnswer(heading, answers);
        Console.Write($"  \"{label}\" → ");

        // Choice questions are checked before text ones: a multiple-choice question with an
        // "Other:" option also contains a text input, and clicking the option is what matters.
        var radios = Visible(question, RadioSelector);
        if (radios.Count > 0)
        {
            ClickOption(js, radios, answer ?? "Yes", "selected");
            return;
        }

        var checkboxes = Visible(question, CheckboxSelector);
        if (checkboxes.Count > 0)
        {
            ClickOption(js, checkboxes, answer ?? "Yes", "checked");
            return;
        }

        var listbox = Visible(question, ListboxSelector).FirstOrDefault();
        if (listbox != null)
        {
            SelectFromDropdown(js, listbox, answer);
            return;
        }

        var textInput = Visible(question, TextInputSelector).FirstOrDefault();
        if (textInput != null)
        {
            var value = answer ?? "yes";
            textInput.Clear();
            textInput.SendKeys(value);
            Console.WriteLine($"filled \"{value}\"");
            return;
        }

        Console.WriteLine("(no interactive input found)");
    }

    private static void ClickOption(
        IJavaScriptExecutor js,
        List<IWebElement> options,
        string desired,
        string verb)
    {
        foreach (var option in options)
        {
            var optionLabel = GetOptionLabel(option);
            if (optionLabel.Contains(desired, StringComparison.OrdinalIgnoreCase))
            {
                js.ExecuteScript("arguments[0].click()", option);
                Console.WriteLine($"{verb} \"{optionLabel}\"");
                return;
            }
        }

        js.ExecuteScript("arguments[0].click()", options[0]);
        Console.WriteLine($"{verb} first option \"{GetOptionLabel(options[0])}\" (no \"{desired}\" found)");
    }

    private void SelectFromDropdown(IJavaScriptExecutor js, IWebElement listbox, string? desired)
    {
        js.ExecuteScript("arguments[0].click()", listbox);

        // The first option is Google's "Choose" placeholder, so it is never a valid answer.
        var options = _driver.FindElements(By.CssSelector(OptionSelector))
            .Where(o => o.Displayed && !string.IsNullOrEmpty(GetOptionLabel(o)))
            .Skip(1)
            .ToList();

        if (options.Count == 0)
        {
            Console.WriteLine("dropdown — no options found");
            return;
        }

        ClickOption(js, options, desired ?? "Yes", "picked");
    }

    /// <summary>
    /// Finds a Google Forms action button by its exact label. Matching exactly matters: the
    /// footer also holds a "Clear form" button that must never be clicked.
    /// </summary>
    private IWebElement? FindButton(string label)
    {
        foreach (var button in _driver.FindElements(By.CssSelector(ButtonSelector)))
        {
            if (!button.Displayed || !button.Enabled) continue;

            var ariaLabel = button.GetDomAttribute("aria-label") ?? "";
            if (ariaLabel.Length > 0)
            {
                if (ariaLabel.Equals(label, StringComparison.OrdinalIgnoreCase)) return button;
                continue;
            }

            if ((button.Text ?? "").Trim().Equals(label, StringComparison.OrdinalIgnoreCase)) return button;
        }

        return null;
    }

    private static List<IWebElement> Visible(IWebElement container, string selector) =>
        container.FindElements(By.CssSelector(selector))
            .Where(e => e.Displayed && e.Enabled)
            .ToList();

    private static string GetOptionLabel(IWebElement option) =>
        (option.GetDomAttribute("aria-label")
         ?? option.GetDomAttribute("data-value")
         ?? option.Text
         ?? "").Trim();

    private static string GetHeadingText(IWebElement question)
    {
        var heading = question.FindElements(By.CssSelector(HeadingSelector)).FirstOrDefault();
        return heading?.Text ?? "";
    }

    /// <summary>
    /// Google Forms crams any preamble into the same heading as the question, so the
    /// actual question is the last non-empty line. Required fields get a trailing "*".
    /// </summary>
    public static string CleanLabel(string? headingText)
    {
        var lines = (headingText ?? "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(l => l.TrimEnd('*', ' ', '\t').Trim())
            .Where(l => l.Length > 0)
            .ToList();

        return lines.Count > 0 ? lines[^1] : "";
    }

    /// <summary>
    /// Like <see cref="FormPopulatorBase.MatchAnswer"/>, but returns the longest — and so most
    /// specific — matching key instead of the first. Google's wordy labels make first-match
    /// wrong surprisingly often: "Telegram Number Or Username" contains "name".
    /// </summary>
    public static string? MatchBestAnswer(string label, Dictionary<string, string> answers)
    {
        string? best = null;
        int bestKeyLength = 0;

        foreach (var kv in answers)
        {
            if (kv.Key.Length > bestKeyLength && label.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
            {
                best = kv.Value;
                bestKeyLength = kv.Key.Length;
            }
        }

        return best;
    }
}
