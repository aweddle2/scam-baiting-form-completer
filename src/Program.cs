using OpenQA.Selenium.Chrome;
using ScamBaitingFormCompleter.FormPopulators;

var urls = new[]
{
    //"https://form.jotform.com/261895272263060",
    //"https://form.jotform.com/261687136210050",
    //"https://forms.office.com/pages/responsepage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAN__7QyVexURVE1UDc0NVRFUjQ4UUNUVFk3S0FCSjQ1Ry4u&origin=lprLink&route=shorturl",
    //"https://forms.office.com/pages/responsepage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAZAAA3614JUMDlUUEZRRjU2TEdCMVpOSVBIN1UyRjNTTC4u&origin=lprLink&route=shorturl",
    "https://docs.google.com/forms/d/e/1FAIpQLSf4VxbmZ7xQ4jm81LbhEV3CaoSSiPdObj13bIsQqzi6zR-nKw/viewform"
};

int runCount = args.Length > 0 && int.TryParse(args[0], out var parsedCount) ? parsedCount : 10;

using (var driver = CreateDriver())
{
    foreach (var url in urls)
    {
        Console.WriteLine($"URL: {url}");
        FormPopulatorFactory.Create(url, driver).Run(url, runCount);
    }
}

static ChromeDriver CreateDriver()
{
    var options = new ChromeOptions();
    if (!System.Diagnostics.Debugger.IsAttached) options.AddArgument("--headless=new");
    options.AddArgument("--start-maximized");
    return new ChromeDriver(options);
}
