namespace DevTest.Trades.Services
{
    public interface IConfigurationProvider
    {
        string OutputFolder { get; set; }
        int Interval { get; set; }
    }
}