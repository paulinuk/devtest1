using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services;

namespace DevTest.Trades.Services.Interfaces
{
    public interface ITradeExtractorService
    {
        void Start();
        void Stop();
        Task<IEnumerable<PowerPeriod>> ProcessForDateTimeAsync(DateTime dateTime, IEnumerable<PowerTrade> specificTrades = null);
    }
}
