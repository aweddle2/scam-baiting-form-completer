public static class FormAnswers
{
    public static Dictionary<string, string> Build()
    {
        var name = FormData.Names[Random.Shared.Next(FormData.Names.Length)];
        var joinedRaw = string.Concat(name.ToLowerInvariant().Split(' '));
        var joinedSanitized = new string(joinedRaw
            .Where(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or '+').ToArray());
        var year = Random.Shared.Next(1950, 2006);
        var provider = FormData.EmailProviders[Random.Shared.Next(FormData.EmailProviders.Length)];
        var email = $"{joinedRaw}{year}@{provider}";
        var nationality = FormData.Countries[Random.Shared.Next(FormData.Countries.Length)];

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]                                               = name,
            ["Please provide your full name"]                      = name,
            ["email"]                                              = email,
            ["Provide your Email"]                                 = email,
            ["company"]                                            = FormData.Ftse100Companies[Random.Shared.Next(FormData.Ftse100Companies.Length)],
            ["gender"]                                             = FormData.GenderIdentities[Random.Shared.Next(FormData.GenderIdentities.Length)],
            ["phone"]                                              = $"({Random.Shared.Next(200, 1000)}) {Random.Shared.Next(200, 1000)}-{Random.Shared.Next(0, 10000):D4}",
            ["whatsapp"]                                           = $"+1{Random.Shared.Next(200, 1000)}{Random.Shared.Next(200, 1000)}{Random.Shared.Next(0, 10000):D4}",
            ["phoneNumber"]                                        = Random.Shared.Next(200, 1000).ToString() + Random.Shared.Next(0, 10000).ToString("D4"),
            ["areaCode"]                                           = Random.Shared.Next(200, 1000).ToString(),
            ["Current Job"]                                        = FormData.JobTitles[Random.Shared.Next(FormData.JobTitles.Length)],
            ["age"]                                                = Random.Shared.Next(18, 66).ToString(),
            ["nationality"]                                        = nationality,
            ["experience"]                                         = $"{nationality}. No prior experience, fresher.",
            ["location"]                                           = FormData.Cities[Random.Shared.Next(FormData.Cities.Length)],
            ["telegram"]                                           = $"@{joinedSanitized}{Random.Shared.Next(100, 999)}",
            ["What type of work schedule are you interested in ?"] = "Part time",
            ["interested"]                                         = "Yes",
            ["previous"]                                           = "No",
            ["applied"]                                            = "No",
            ["similar"]                                            = "No",
        };
    }
}
