using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public abstract class FormPopulatorBase : IFormPopulator
{
    protected readonly IWebDriver _driver;
    protected readonly WebDriverWait _wait;

    protected FormPopulatorBase(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;
    }

    public void Run(string url, int runCount)
    {
        for (int run = 1; run <= runCount; run++)
        {
            Console.WriteLine($"\n=== Run {run}/{runCount} ===");
            if (!RunIteration(url, FormAnswers.Build())) break;
            WaitBetweenRuns();
        }
        Console.WriteLine($"\nAll run(s) complete.");
    }

    protected abstract bool RunIteration(string url, Dictionary<string, string> answers);

    protected virtual void WaitBetweenRuns()
    {
        var waitSeconds = Random.Shared.Next(2, 12);
        Console.WriteLine($"  Waiting {waitSeconds}s before next run…");
        Thread.Sleep(waitSeconds * 1000);
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Console.WriteLine("  [DEBUG] Press Enter to continue…");
            Console.ReadLine();
        }
    }

    protected void WaitForFormLoad()
    {
        Console.WriteLine("  Waiting for form to load…");
        _wait.Until(d =>
        {
            try
            {
                var loadingElements = d.FindElements(By.CssSelector("[class*='loading'], [aria-label='Loading']"));
                return loadingElements.All(e => !e.Displayed);
            }
            catch (StaleElementReferenceException) { return false; }
        });
    }

    protected static string? MatchAnswer(string label, Dictionary<string, string> answers)
    {
        foreach (var kv in answers)
            if (label.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        return null;
    }
}
