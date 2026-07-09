using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public static class FormPopulatorFactory
{
    public static IFormPopulator Create(string url, IWebDriver driver)
    {
        var host = new Uri(url).Host;
        var wait = CreateWait(driver);

        if (host.EndsWith("jotform.com", StringComparison.OrdinalIgnoreCase))
            return new JotFormPopulator(driver, wait);

        if (host.EndsWith("forms.office.com", StringComparison.OrdinalIgnoreCase))
            return new MicrosoftFormsPopulator(driver, wait);

        if (host.EndsWith("forms.fillout.com", StringComparison.OrdinalIgnoreCase))
            return new FilloutFormPopulator(driver, wait);

        throw new NotSupportedException($"No form populator for domain: {host}");
    }

    private static WebDriverWait CreateWait(IWebDriver driver)
    {
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        return new WebDriverWait(driver, TimeSpan.FromSeconds(30));
    }
}
