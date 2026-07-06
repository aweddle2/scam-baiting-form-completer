var urls = new[]
{
    //"https://form.jotform.com/261687136210050",
    "https://forms.office.com/pages/responsepage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAZAAA3614JUMDlUUEZRRjU2TEdCMVpOSVBIN1UyRjNTTC4u&origin=lprLink&route=shorturl",
};

int runCount = args.Length > 0 && int.TryParse(args[0], out var parsedCount) ? parsedCount : 10;

foreach (var url in urls)
{
    Console.WriteLine($"\n{'=' * 60}");
    Console.WriteLine($"URL: {url}");
    Console.WriteLine($"{'=' * 60}");
    FormPopulatorFactory.Create(url).Run(url, runCount);
}
