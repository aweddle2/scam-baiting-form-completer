namespace ScamBaitingFormCompleter.FormPopulators;

public interface IFormPopulator
{
    void Run(string url, int runCount);
}
