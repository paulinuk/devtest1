using DevTest.Trades.Common.Helpers;
using DevTest.Trades.Services.Interfaces;
using Topshelf;
using Topshelf.Autofac;

namespace DevTest.Trades.WindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = AutoFacHelper.SetupContainer();
            HostFactory.Run(x => //1
            {
                x.UseAutofacContainer(container);
                x.Service<ITradeExtractorService>(s => //2
                {
                    s.ConstructUsingAutofacContainer();
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.StartAutomatically();
                x.RunAsLocalSystem();
                x.EnableServiceRecovery(r => { r.RestartService(0); });
                x.SetDescription($"Service to extract trade position data");
                x.SetDisplayName($"Trade Extractor Service");
                x.SetServiceName($"TradeExtractorWindowsService");
            });
        }
    }
}