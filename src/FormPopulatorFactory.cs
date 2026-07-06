public static class FormPopulatorFactory
{
    public static IFormPopulator Create(string url)
    {
        var host = new Uri(url).Host;

        if (host.EndsWith("jotform.com", StringComparison.OrdinalIgnoreCase))
            return new JotFormPopulator();

        if (host.EndsWith("forms.office.com", StringComparison.OrdinalIgnoreCase))
            return new MicrosoftFormsPopulator();

        if (host.EndsWith("forms.fillout.com", StringComparison.OrdinalIgnoreCase))
            return new FilloutFormPopulator();

        throw new NotSupportedException($"No form populator for domain: {host}");
    }
}
