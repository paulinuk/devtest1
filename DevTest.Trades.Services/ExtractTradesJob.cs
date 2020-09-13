using DevTest.Trades.Common;
using DevTest.Trades.Services.Interfaces;
using Quartz;
using System;
using System.Threading.Tasks;

namespace DevTest.Trades.Services
{
    public sealed class ExtractTradesJob : IJob
    {
        private ITradeExtractorService _tradeExtractorService;

        void IJob.Execute(IJobExecutionContext context)
        {
            var schedulerContext = context.Scheduler.Context;
            _tradeExtractorService = (TradeExtractorService)schedulerContext.Get(Constants.TradeExtractorKey);

            //Newer versions of Quartz support async out of the box, but they are not compatible with .NET 4.5
            Task.Run(async () => await _tradeExtractorService.ProcessForDateTimeAsync(DateTime.Now.ToUniversalTime()));
        }
    }
}