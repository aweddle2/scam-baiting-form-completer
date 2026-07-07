var urls = new[]
{
    "https://form.jotform.com/261687136210050",
    "https://forms.office.com/pages/responsepage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAZAAA3614JUMDlUUEZRRjU2TEdCMVpOSVBIN1UyRjNTTC4u&origin=lprLink&route=shorturl", //this form has reached it's limits
    "https://forms.fillout.com/t/9GAFdjS6fkus",//  This has been deleted, but useful for when looking at automating a skip if it has been deleted.
    "https://forms.office.com/Pages/ResponsePage.aspx?id=aAVLy6WmBUWIJuDJnTz9KdTSzBojYSlEgTuqV9yyWG9UOVdZSVRSUTZYWTFPSEhDQ05MSVFYUlc2Ry4u" //this form is closed
};

int runCount = args.Length > 0 && int.TryParse(args[0], out var parsedCount) ? parsedCount : 10;

foreach (var url in urls)
{
    Console.WriteLine($"\n{'=' * 60}");
    Console.WriteLine($"URL: {url}");
    Console.WriteLine($"{'=' * 60}");
    FormPopulatorFactory.Create(url).Run(url, runCount);
}
