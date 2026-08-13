using Moq;
using OpenQA.Selenium;
using ScamBaitingFormCompleter.FormPopulators;
using Xunit;

namespace ScamBaitingFormCompleter.Tests.FormPopulators;

public class FormPopulatorFactoryTests
{
    [Theory]
    [InlineData("https://docs.google.com/forms/d/e/1FAIpQLSf4VxbmZ7xQ4jm81LbhEV3CaoSSiPdObj13bIsQqzi6zR-nKw/viewform",
        typeof(GoogleFormsPopulator))]
    [InlineData("https://forms.gle/abc123", typeof(GoogleFormsPopulator))]
    [InlineData("https://form.jotform.com/261895272263060", typeof(JotFormPopulator))]
    [InlineData("https://forms.office.com/pages/responsepage.aspx?id=example", typeof(MicrosoftFormsPopulator))]
    [InlineData("https://forms.fillout.com/t/example", typeof(FilloutFormPopulator))]
    public void Create_ReturnsThePopulatorForTheHost(string url, Type expected)
    {
        var populator = FormPopulatorFactory.Create(url, CreateDriver());

        Assert.IsType(expected, populator);
    }

    [Fact]
    public void Create_UnknownHost_Throws()
    {
        var ex = Assert.Throws<NotSupportedException>(
            () => FormPopulatorFactory.Create("https://example.com/some-form", CreateDriver()));

        Assert.Contains("example.com", ex.Message);
    }

    private static IWebDriver CreateDriver()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        return driver.Object;
    }
}
