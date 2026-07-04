public static class FormPopulatorFactory
{
    public static IFormPopulator Create(string url)
    {
        var uri = new Uri(url);
        return uri.Host.EndsWith("jotform.com", StringComparison.OrdinalIgnoreCase)
            ? new JotFormPopulator()
            : throw new NotSupportedException($"No form populator for domain: {uri.Host}");
    }
}
