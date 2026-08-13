using System.Collections.ObjectModel;
using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScamBaitingFormCompleter.FormPopulators;
using Xunit;

namespace ScamBaitingFormCompleter.Tests.FormPopulators;

public class GoogleFormsPopulatorTests
{
    private const string FormUrl =
        "https://docs.google.com/forms/d/e/1FAIpQLSf4VxbmZ7xQ4jm81LbhEV3CaoSSiPdObj13bIsQqzi6zR-nKw/viewform";

    /// <summary>
    /// The twelve question headings from the live form, in order, exactly as Selenium reads
    /// them: the preamble Google packs into the first question's heading, the "*" required
    /// marker, and all. Every question on this form is a short-answer text field.
    /// </summary>
    public static readonly string[] RealFormHeadings =
    [
        "We are looking for motivated individuals to assist with online operations for the following roles.\n"
            + "-Remote Application consultant\n"
            + "-App review Assistant\n"
            + "This is a flexible, remote opportunity with training provided. Pay: $180–$250 per day + bonuses "
            + "Schedule: Part-time or full-timework Type: 100% remote  Support AI systems• Ensure accuracy and "
            + "efficiency Requirements:• 20+ years old• Basic English• Comfortable using online tools, No "
            + "experience required — full training provided\n"
            + "Are you interested in this position? *",
        "Provide your Name: *",
        "Age: *",
        "Gender: *",
        "Email Address:",
        "WhatsApp Number: *",
        "Provide Your Telegram Number Or Username: (We use telegram during interview and documents request "
            + "process, if you don’t have telegram installed kindly download it before proceeding) *",
        "Nationality: *",
        "Current Location: *",
        "Have you previously pursued opportunities similar to this position? *",
        "Current job (if you are working actually ) *",
        "Kindly  send a confirmation an email regarding your application @ angelinalove19870@gmail.com. This "
            + "will allow me to arrange an interview and Forward your details to our representative. Thank you "
            + "for your time and cooperation. \n"
            + "Best regards *",
    ];

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        var wait = new WebDriverWait(driver.Object, TimeSpan.FromSeconds(1));

        var populator = new GoogleFormsPopulator(driver.Object, wait);

        Assert.NotNull(populator);
    }

    [Fact(Skip = "Requires a real browser and network access")]
    public void Run_GoogleFormsUrl_CompletesAllRuns()
    {
        var driver = new Mock<IWebDriver>();
        driver.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        var wait = new WebDriverWait(driver.Object, TimeSpan.FromSeconds(1));

        new GoogleFormsPopulator(driver.Object, wait).Run(FormUrl, runCount: 1);
    }

    // ---------------------------------------------------------------------------------
    // Proof the form gets filled in and submitted
    // ---------------------------------------------------------------------------------

    [Fact]
    public void Run_RealFormLayout_FillsEveryQuestionAndSubmits()
    {
        var form = new FakeGoogleForm(RealFormHeadings.Select(FakeQuestion.Text).ToArray());

        form.CreatePopulator().Run(FormUrl, runCount: 1);

        Assert.Equal(12, form.Questions.Count);
        Assert.All(form.Questions, q =>
        {
            Assert.Single(q.Typed);
            Assert.False(string.IsNullOrWhiteSpace(q.Typed[0]));
        });

        Assert.True(form.SubmitWasClicked, "the Submit button should have been clicked");
    }

    [Fact]
    public void Run_RealFormLayout_AnswersEachQuestionInCharacter()
    {
        var form = new FakeGoogleForm(RealFormHeadings.Select(FakeQuestion.Text).ToArray());

        form.CreatePopulator().Run(FormUrl, runCount: 1);

        Assert.Equal("Yes", Answer(form, "Are you interested in this position?"));
        // Two words, allowing the hyphens and apostrophes the name list contains.
        Assert.Matches(@"^[\p{L}'\-]+ [\p{L}'\-]+$", Answer(form, "Provide your Name:"));
        Assert.InRange(int.Parse(Answer(form, "Age:")), 18, 65);
        Assert.Contains("@", Answer(form, "Email Address:"));
        Assert.StartsWith("+1", Answer(form, "WhatsApp Number:"));
        // Proves the longest-key match beat "name" inside "Username".
        Assert.StartsWith("@", Answer(form, "Telegram"));
        Assert.Equal("No", Answer(form, "Have you previously pursued"));
    }

    [Fact]
    public void Run_RealFormLayout_NeverClicksClearForm()
    {
        var form = new FakeGoogleForm(RealFormHeadings.Select(FakeQuestion.Text).ToArray());

        form.CreatePopulator().Run(FormUrl, runCount: 1);

        Assert.DoesNotContain(form.ClearButton.Object, form.JsClicks);
    }

    [Fact]
    public void Run_ChoiceQuestions_SelectsTheMatchingOption()
    {
        var interested = FakeQuestion.Radio("Are you interested in this position? *", "Yes", "No");
        var previous = FakeQuestion.Radio("Have you previously pursued opportunities similar to this? *", "Yes", "No");
        var form = new FakeGoogleForm(interested, previous);

        form.CreatePopulator().Run(FormUrl, runCount: 1);

        Assert.Equal("Yes", form.ClickedOptionOf(interested));
        Assert.Equal("No", form.ClickedOptionOf(previous));
        Assert.True(form.SubmitWasClicked);
    }

    [Fact]
    public void Run_MultiPageForm_FillsBothPagesThenSubmits()
    {
        var page1 = new[] { FakeQuestion.Text("Provide your Name: *") };
        var page2 = new[] { FakeQuestion.Text("Email Address: *") };
        var form = new FakeGoogleForm(page1, page2);

        form.CreatePopulator().Run(FormUrl, runCount: 1);

        Assert.Single(page1[0].Typed);
        Assert.Single(page2[0].Typed);
        Assert.Contains("@", page2[0].Typed[0]);
        Assert.True(form.NextWasClicked, "the Next button should have been clicked");
        Assert.True(form.SubmitWasClicked);
    }

    [Fact]
    public void Run_WhenFormNoLongerAcceptingResponses_StopsAfterOneIteration()
    {
        var driverMock = new Mock<IWebDriver>();

        // As<> must be called before .Object is first accessed
        driverMock.As<IJavaScriptExecutor>()
            .Setup(js => js.ExecuteScript("return arguments[0].textContent", It.IsAny<object[]>()))
            .Returns("The form Job application form is no longer accepting responses.");

        driverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
        driverMock.Setup(d => d.FindElements(It.IsAny<By>())).Returns(Wrap());

        var navMock = new Mock<INavigation>();
        driverMock.Setup(d => d.Navigate()).Returns(navMock.Object);

        // IsFormClosed: a heading whose JS text content carries the closed-form message
        driverMock
            .Setup(d => d.FindElements(By.CssSelector(GoogleFormsPopulator.ClosedHeadingSelector)))
            .Returns(Wrap(new Mock<IWebElement>().Object));

        var wait = new WebDriverWait(driverMock.Object, TimeSpan.FromSeconds(1));
        var populator = new GoogleFormsPopulator(driverMock.Object, wait);

        populator.Run(FormUrl, runCount: 3);

        // Only one navigation should occur — the loop exits on first closed-form detection
        navMock.Verify(n => n.GoToUrl(FormUrl), Times.Once);
    }

    // ---------------------------------------------------------------------------------
    // Label parsing and answer matching
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, "interested")]
    [InlineData(1, "name")]
    [InlineData(2, "age")]
    [InlineData(3, "gender")]
    [InlineData(4, "email")]
    [InlineData(5, "whatsapp")]
    [InlineData(6, "telegram")]
    [InlineData(7, "nationality")]
    [InlineData(8, "location")]
    [InlineData(9, "previous")]
    [InlineData(10, "Current Job")]
    [InlineData(11, "email")]
    public void RealFormHeading_ResolvesToTheExpectedAnswer(int index, string expectedAnswerKey)
    {
        var answers = FormAnswers.Build();
        var heading = RealFormHeadings[index];

        var resolved = GoogleFormsPopulator.MatchBestAnswer(GoogleFormsPopulator.CleanLabel(heading), answers)
                       ?? GoogleFormsPopulator.MatchBestAnswer(heading, answers);

        Assert.Equal(answers[expectedAnswerKey], resolved);
    }

    [Fact]
    public void CleanLabel_TakesTheQuestionFromTheEndOfAWordyHeading()
    {
        Assert.Equal("Are you interested in this position?",
            GoogleFormsPopulator.CleanLabel(RealFormHeadings[0]));
    }

    [Theory]
    [InlineData("Provide your Name: *", "Provide your Name:")]
    [InlineData("Age:   *  ", "Age:")]
    [InlineData("Nationality:", "Nationality:")]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("*", "")]
    public void CleanLabel_StripsTheRequiredMarkerAndWhitespace(string? heading, string expected)
    {
        Assert.Equal(expected, GoogleFormsPopulator.CleanLabel(heading));
    }

    [Fact]
    public void MatchBestAnswer_PrefersTheMostSpecificKey()
    {
        var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Alice Hartman",
            ["telegram"] = "@alicehartman123",
        };

        // "Username" contains "name", so first-match would return the wrong answer here.
        Assert.Equal("@alicehartman123",
            GoogleFormsPopulator.MatchBestAnswer("Provide Your Telegram Number Or Username:", answers));
    }

    [Fact]
    public void MatchBestAnswer_ReturnsNullWhenNothingMatches()
    {
        var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["name"] = "Alice Hartman" };

        Assert.Null(GoogleFormsPopulator.MatchBestAnswer("Best regards", answers));
    }

    // ---------------------------------------------------------------------------------
    // Fake Google Forms DOM
    // ---------------------------------------------------------------------------------

    private static string Answer(FakeGoogleForm form, string headingFragment) =>
        form.Questions.First(q => q.Heading.Contains(headingFragment, StringComparison.Ordinal)).Typed.Single();

    private static ReadOnlyCollection<IWebElement> Wrap(params IWebElement[] elements) => new(elements);

    /// <summary>
    /// Suppresses the real between-run and post-submit sleeps so the tests run instantly.
    /// </summary>
    private sealed class TestableGoogleFormsPopulator(IWebDriver driver, WebDriverWait wait)
        : GoogleFormsPopulator(driver, wait)
    {
        protected override void WaitBetweenRuns() { }
        protected override void PauseAfterSubmit() { }
    }

    /// <summary>
    /// One question, mirroring the DOM Google Forms produces: a role='listitem' container
    /// holding a role='heading' label plus either a text input or role='radio' options.
    /// </summary>
    private sealed class FakeQuestion
    {
        public string Heading { get; }
        public Mock<IWebElement> Container { get; } = new();
        public List<string> Typed { get; } = [];
        public Dictionary<string, Mock<IWebElement>> Options { get; } = [];

        /// <summary>Interactive elements the driver-level readiness check should see.</summary>
        public List<IWebElement> Fields { get; } = [];

        private FakeQuestion(string heading)
        {
            Heading = heading;

            var headingElement = new Mock<IWebElement>();
            headingElement.SetupGet(h => h.Text).Returns(heading);

            Container.SetupGet(c => c.Displayed).Returns(true);
            Container.SetupGet(c => c.Enabled).Returns(true);
            // Anything the populator looks for that this question doesn't have comes back empty.
            Container.Setup(c => c.FindElements(It.IsAny<By>())).Returns(Wrap());
            Container.Setup(c => c.FindElements(By.CssSelector(GoogleFormsPopulator.HeadingSelector)))
                .Returns(Wrap(headingElement.Object));
        }

        public static FakeQuestion Text(string heading)
        {
            var question = new FakeQuestion(heading);

            var input = new Mock<IWebElement>();
            input.SetupGet(i => i.Displayed).Returns(true);
            input.SetupGet(i => i.Enabled).Returns(true);
            input.Setup(i => i.SendKeys(It.IsAny<string>())).Callback<string>(question.Typed.Add);

            question.Fields.Add(input.Object);
            question.Container
                .Setup(c => c.FindElements(By.CssSelector(GoogleFormsPopulator.TextInputSelector)))
                .Returns(Wrap(input.Object));

            return question;
        }

        public static FakeQuestion Radio(string heading, params string[] optionLabels)
        {
            var question = new FakeQuestion(heading);

            foreach (var optionLabel in optionLabels)
            {
                var option = new Mock<IWebElement>();
                option.SetupGet(o => o.Displayed).Returns(true);
                option.SetupGet(o => o.Enabled).Returns(true);
                option.Setup(o => o.GetDomAttribute("aria-label")).Returns(optionLabel);
                question.Options[optionLabel] = option;
                question.Fields.Add(option.Object);
            }

            // Ordered by the caller's option order, so "first option" fallbacks are meaningful.
            question.Container
                .Setup(c => c.FindElements(By.CssSelector(GoogleFormsPopulator.RadioSelector)))
                .Returns(Wrap(optionLabels.Select(l => question.Options[l].Object).ToArray()));

            return question;
        }
    }

    /// <summary>
    /// A whole fake Google Form: pages of questions, a Submit button, a Clear form button
    /// (which must never be clicked) and, for multi-page forms, a Next button that goes away
    /// once used — just like the real thing.
    /// </summary>
    private sealed class FakeGoogleForm
    {
        private readonly IReadOnlyList<FakeQuestion[]> _pages;
        private int _currentPage;

        public Mock<IWebDriver> DriverMock { get; } = new();
        public Mock<INavigation> NavigationMock { get; } = new();
        public Mock<IWebElement> SubmitButton { get; } = Button("Submit");
        public Mock<IWebElement> NextButton { get; } = Button("Next");
        // The live form's Clear form button carries no aria-label — it is only identifiable
        // by its text, which is exactly the case that could be mistaken for an action button.
        public Mock<IWebElement> ClearButton { get; } = TextOnlyButton("Clear form");
        public List<IWebElement> JsClicks { get; } = [];

        public FakeGoogleForm(params FakeQuestion[] questions) : this([questions]) { }

        public FakeGoogleForm(params FakeQuestion[][] pages) : this((IReadOnlyList<FakeQuestion[]>)pages) { }

        private FakeGoogleForm(IReadOnlyList<FakeQuestion[]> pages)
        {
            _pages = pages;

            // As<> must be called before .Object is first accessed
            var js = DriverMock.As<IJavaScriptExecutor>();
            js.Setup(x => x.ExecuteScript("return arguments[0].textContent", It.IsAny<object[]>()))
                .Returns("");
            js.Setup(x => x.ExecuteScript("arguments[0].click()", It.IsAny<object[]>()))
                .Callback<string, object[]>((_, args) => RecordClick(args));

            DriverMock.Setup(d => d.Manage().Timeouts().ImplicitWait).Returns(TimeSpan.Zero);
            DriverMock.Setup(d => d.Navigate()).Returns(NavigationMock.Object);

            // Anything not explicitly present on the page comes back empty.
            DriverMock.Setup(d => d.FindElements(It.IsAny<By>())).Returns(Wrap());

            DriverMock.Setup(d => d.FindElements(By.CssSelector(GoogleFormsPopulator.QuestionSelector)))
                .Returns(() => Wrap(CurrentQuestions.Select(q => q.Container.Object).ToArray()));

            DriverMock.Setup(d => d.FindElements(By.CssSelector(GoogleFormsPopulator.FieldSelector)))
                .Returns(() => Wrap(CurrentQuestions.SelectMany(q => q.Fields).ToArray()));

            DriverMock.Setup(d => d.FindElements(By.CssSelector(GoogleFormsPopulator.ButtonSelector)))
                .Returns(() => Wrap(CurrentButtons));
        }

        public List<FakeQuestion> Questions => _pages.SelectMany(p => p).ToList();

        public bool SubmitWasClicked => JsClicks.Contains(SubmitButton.Object);

        public bool NextWasClicked => JsClicks.Contains(NextButton.Object);

        public GoogleFormsPopulator CreatePopulator() =>
            new TestableGoogleFormsPopulator(
                DriverMock.Object,
                new WebDriverWait(DriverMock.Object, TimeSpan.FromSeconds(1)));

        /// <summary>The option label the populator actually clicked for a choice question.</summary>
        public string? ClickedOptionOf(FakeQuestion question) =>
            question.Options.FirstOrDefault(o => JsClicks.Contains(o.Value.Object)).Key;

        private IEnumerable<FakeQuestion> CurrentQuestions => _pages[_currentPage];

        // Google shows Next on every page but the last, and Submit only on the last.
        private IWebElement[] CurrentButtons =>
            _currentPage < _pages.Count - 1
                ? [NextButton.Object, ClearButton.Object]
                : [SubmitButton.Object, ClearButton.Object];

        private void RecordClick(object[] args)
        {
            if (args.Length == 0 || args[0] is not IWebElement element) return;

            JsClicks.Add(element);
            if (ReferenceEquals(element, NextButton.Object) && _currentPage < _pages.Count - 1)
                _currentPage++;
        }

        private static Mock<IWebElement> Button(string ariaLabel)
        {
            var button = BareButton();
            button.Setup(b => b.GetDomAttribute("aria-label")).Returns(ariaLabel);
            return button;
        }

        private static Mock<IWebElement> TextOnlyButton(string text)
        {
            var button = BareButton();
            button.Setup(b => b.GetDomAttribute("aria-label")).Returns((string)null!);
            button.SetupGet(b => b.Text).Returns(text);
            return button;
        }

        private static Mock<IWebElement> BareButton()
        {
            var button = new Mock<IWebElement>();
            button.SetupGet(b => b.Displayed).Returns(true);
            button.SetupGet(b => b.Enabled).Returns(true);
            return button;
        }
    }
}
