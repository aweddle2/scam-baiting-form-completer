const string FormUrl = "https://form.jotform.com/261687136210050";
int runCount = args.Length > 0 && int.TryParse(args[0], out var parsedCount) ? parsedCount : 10;

var populator = FormPopulatorFactory.Create(FormUrl);
populator.Run(FormUrl, runCount);
