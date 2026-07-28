using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScamBaitingFormCompleter.FormPopulators;
using Xunit;

namespace ScamBaitingFormCompleter.Tests.FormPopulators;

public class JotFormPopulatorTests
{
    private readonly Mock<IWebDriver> _driverMock;
    private readonly WebDriverWait _wait;
    private readonly JotFormPopulator _populator;

    public JotFormPopulatorTests()
    {
        _driverMock = new Mock<IWebDriver>();
        _driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        _wait = new WebDriverWait(_driverMock.Object, TimeSpan.FromSeconds(1));
        _populator = new JotFormPopulator(_driverMock.Object, _wait);
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        var wait = new WebDriverWait(driver.Object, TimeSpan.FromSeconds(1));

        var populator = new JotFormPopulator(driver.Object, wait);

        Assert.NotNull(populator);
    }

    [Fact(Skip = "Requires a real browser and network access")]
    public void Run_JotFormUrl_CompletesAllRuns()
    {
        _populator.Run("https://form.jotform.com/123456789", runCount: 1);
    }

    [Fact]
    public void Run_WhenFormOverQuota_StopsAfterOneIteration()
    {
        const string url = "https://form.jotform.com/261895272263060";

        var driverMock = new Mock<IWebDriver>();

        // As<> must be called before .Object is first accessed
        driverMock.As<IJavaScriptExecutor>()
            .Setup(js => js.ExecuteScript("return arguments[0].textContent", It.IsAny<object[]>()))
            .Returns("Form over quota");

        driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);

        var navMock = new Mock<INavigation>();
        driverMock.Setup(d => d.Navigate()).Returns(navMock.Object);

        // WaitForForm: return one element so the wait condition (count > 0) is satisfied
        var waitElement = new Mock<IWebElement>();
        driverMock
            .Setup(d => d.FindElements(By.CssSelector(
                "li.form-line[data-type], div.form-line[data-type], [data-type*='control_']," +
                " input[id^='input_'], textarea[id^='input_'], h2[class='modal-heading-title']")))
            .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(
                [waitElement.Object]));

        // IsOverQuota: one h2 element whose JS text content is "Form over quota"
        var quotaElement = new Mock<IWebElement>();
        driverMock
            .Setup(d => d.FindElements(By.CssSelector("h2.modal-heading-title")))
            .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(
                [quotaElement.Object]));

        var wait = new WebDriverWait(driverMock.Object, TimeSpan.FromSeconds(1));
        var populator = new JotFormPopulator(driverMock.Object, wait);

        populator.Run(url, runCount: 3);

        // Only one navigation should occur — the loop exits on first over-quota detection
        navMock.Verify(n => n.GoToUrl(url), Times.Once);
    }
}
