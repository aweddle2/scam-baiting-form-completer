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

    [Fact(Skip = "Requires a real browser and network access")]
    public void Run_WhenFormOverQuota_StopsEarly()
    {
        _populator.Run("https://form.jotform.com/123456789", runCount: 5);
    }
}
