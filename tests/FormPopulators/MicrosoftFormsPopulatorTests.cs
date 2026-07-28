using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScamBaitingFormCompleter.FormPopulators;
using Xunit;

namespace ScamBaitingFormCompleter.Tests.FormPopulators;

public class MicrosoftFormsPopulatorTests
{
    private readonly Mock<IWebDriver> _driverMock;
    private readonly WebDriverWait _wait;
    private readonly MicrosoftFormsPopulator _populator;

    public MicrosoftFormsPopulatorTests()
    {
        _driverMock = new Mock<IWebDriver>();
        _driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        _wait = new WebDriverWait(_driverMock.Object, TimeSpan.FromSeconds(1));
        _populator = new MicrosoftFormsPopulator(_driverMock.Object, _wait);
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        var wait = new WebDriverWait(driver.Object, TimeSpan.FromSeconds(1));

        var populator = new MicrosoftFormsPopulator(driver.Object, wait);

        Assert.NotNull(populator);
    }

    [Fact(Skip = "Requires a real browser and network access")]
    public void Run_MicrosoftFormsUrl_CompletesAllRuns()
    {
        _populator.Run("https://forms.office.com/pages/responsepage.aspx?id=example", runCount: 1);
    }

    [Fact]
    public void Run_WhenFormClosed_StopsAfterOneIteration()
    {
        const string url = "https://forms.office.com/Pages/ResponsePage.aspx?id=aAVLy6WmBUWIJuDJnTz9KdTSzBojYSlEgTuqV9yyWG9UOVdZSVRSUTZYWTFPSEhDQ05MSVFYUlc2Ry4u";

        var driverMock = new Mock<IWebDriver>();

        // As<> must be called before .Object is first accessed
        driverMock.As<IJavaScriptExecutor>()
            .Setup(js => js.ExecuteScript("return arguments[0].textContent", It.IsAny<object[]>()))
            .Returns("This form is closed");

        driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);

        var navMock = new Mock<INavigation>();
        driverMock.Setup(d => d.Navigate()).Returns(navMock.Object);

        // WaitForFormLoad: no loading spinners present
        driverMock
            .Setup(d => d.FindElements(By.CssSelector("[class*='loading'], [aria-label='Loading']")))
            .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(new List<IWebElement>()));

        // IsFormClosed: one errorTitle element whose JS text content is "This form is closed"
        var errorElement = new Mock<IWebElement>();
        driverMock
            .Setup(d => d.FindElements(By.CssSelector("[data-automation-id='errorTitle']")))
            .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>(
                new List<IWebElement> { errorElement.Object }));

        var wait = new WebDriverWait(driverMock.Object, TimeSpan.FromSeconds(1));
        var populator = new MicrosoftFormsPopulator(driverMock.Object, wait);

        populator.Run(url, runCount: 3);

        // Only one navigation should occur — the loop exits on first closed-form detection
        navMock.Verify(n => n.GoToUrl(url), Times.Once);
    }

    [Fact(Skip = "Requires a real browser and network access")]
    public void Run_WhenMaxResponsesReached_StopsEarly()
    {
        _populator.Run("https://forms.office.com/pages/responsepage.aspx?id=max-responses-form", runCount: 5);
    }
}
