using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScamBaitingFormCompleter.FormPopulators;
using Xunit;

namespace ScamBaitingFormCompleter.Tests.FormPopulators;

public class FilloutFormPopulatorTests
{
    private readonly Mock<IWebDriver> _driverMock;
    private readonly WebDriverWait _wait;
    private readonly FilloutFormPopulator _populator;

    public FilloutFormPopulatorTests()
    {
        _driverMock = new Mock<IWebDriver>();
        _driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        _wait = new WebDriverWait(_driverMock.Object, TimeSpan.FromSeconds(1));
        _populator = new FilloutFormPopulator(_driverMock.Object, _wait);
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        var wait = new WebDriverWait(driver.Object, TimeSpan.FromSeconds(1));

        var populator = new FilloutFormPopulator(driver.Object, wait);

        Assert.NotNull(populator);
    }

    [Fact(Skip = "Requires a real browser and network access")]
    public void Run_FilloutUrl_CompletesAllRuns()
    {
        _populator.Run("https://forms.fillout.com/t/example", runCount: 1);
    }

    [Fact]
    public void Run_WhenPageNotFound_StopsAfterOneIteration()
    {
        const string url = "https://forms.fillout.com/t/9GAFdjS6fkus";

        var driverMock = new Mock<IWebDriver>();

        // As<> must be called before .Object is first accessed
        driverMock.As<IJavaScriptExecutor>()
            .Setup(js => js.ExecuteScript("return arguments[0].textContent", It.IsAny<object[]>()))
            .Returns("Page not found");

        driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);

        var navMock = new Mock<INavigation>();
        driverMock.Setup(d => d.Navigate()).Returns(navMock.Object);

        // WaitForFormLoad: no loading spinners present
        driverMock
            .Setup(d => d.FindElements(By.CssSelector("[class*='loading'], [aria-label='Loading']")))
            .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new List<IWebElement>()));

        // IsPageNotFound: one h1 element whose JS text content is "Page not found"
        var h1Element = new Mock<IWebElement>();
        driverMock
            .Setup(d => d.FindElements(By.CssSelector("h1")))
            .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(
                new List<IWebElement> { h1Element.Object }));

        var wait = new WebDriverWait(driverMock.Object, TimeSpan.FromSeconds(1));
        var populator = new FilloutFormPopulator(driverMock.Object, wait);

        populator.Run(url, runCount: 3);

        // Only one navigation should occur — the loop exits on first deleted-form detection
        navMock.Verify(n => n.GoToUrl(url), Times.Once);
    }
}
