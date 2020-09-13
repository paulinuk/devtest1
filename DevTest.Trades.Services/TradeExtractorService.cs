using Catel;
using Catel.Linq;
using DevTest.Trades.Common;
using DevTest.Trades.Services.Interfaces;
using Quartz;
using Serilog;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTest.Trades.Services
{
    public class TradeExtractorService : ITradeExtractorService
    {
        private IScheduler _scheduler;
        private IConfigurationProvider _configurationProvider;
        private IPowerService _powerService;
        private Dictionary<int, string> _periodTextLookup;

        private void SetupPeriodTextLookup()
        {
            _periodTextLookup = new Dictionary<int, string>();

            var date = Convert.ToDateTime(DateTime.Now.Date.AddHours(23));
            for (var i = 1; i <= 24; i++)
            {
                _periodTextLookup.Add(i, date.ToString("HH:mm"));
                date = date.AddHours(1);
            }
        }

        public TradeExtractorService(IScheduler scheduler,
            IConfigurationProvider configurationProvider,
            ILogger log)
        {
            Argument.IsNotNull(() => scheduler);
            Argument.IsNotNull(() => configurationProvider);
            Argument.IsNotNull(() => log);

            _scheduler = scheduler;
            _configurationProvider = configurationProvider;

            Log.Logger = log;
            SetupPeriodTextLookup();

            _powerService = new PowerService();
        }

        public void Start()
        {
            SetupScheduler();
        }

        private void SetupScheduler()
        {
            var minuteText = _configurationProvider.Interval > 1 ? "minutes" : "minute";
            Log.Information($"Setting up scheduled job to extract trades every {_configurationProvider.Interval} {minuteText}");

            var result = TriggerBuilder.Create()
                .WithIdentity("1JobTrigger")
                .WithSimpleSchedule(x => x
                    .RepeatForever()
                    .WithIntervalInMinutes(_configurationProvider.Interval)
                )
                .StartNow()
                .WithPriority(1)
                .Build();

            var job = JobBuilder.Create(typeof(ExtractTradesJob))
                .WithIdentity("ITradeExtractorService")
                .Build();

            _scheduler.ScheduleJob(job, result);
            _scheduler.Start();

            //Pass this instance to the job (DI not supported for jobs)
            _scheduler.Context.Put(Constants.TradeExtractorKey, this);
        }

        public void Stop()
        {
            _scheduler.Shutdown();
        }

        private List<PowerPeriod> SummatePeriods(IEnumerable<PowerTrade> trades, DateTime dateTime)
        {
            if (trades.Any() == false)
            {
                throw new System.Data.DataException($"No trades were found for {dateTime.ToString("yyyyMMdd HH:mm")}");
            }

            var allPeriods = trades.SelectMany(y => y.Periods).ToList();

            var summatedPeriods = allPeriods
                .GroupBy(x => x.Period)
                .Select(g => new PowerPeriod()
                {
                    Period = g.Key,
                    Volume = g.Sum(x => x.Volume)
                }).ToList();

            return summatedPeriods;
        }

        private void OutputToFile(List<PowerPeriod> periods, string filename)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Period,Volume");
            foreach (var period in periods)
            {
                var periodText = _periodTextLookup[period.Period];
                var line = $"{periodText},{period.Volume}";
                stringBuilder.AppendLine(line);
            }

            File.WriteAllText(filename, stringBuilder.ToString());
        }

        private async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime dateTime)
        {
            try
            {
                var result = await _powerService.GetTradesAsync(dateTime);
                return result;
            }
            catch (Exception e)
            {
                //Ensure that as much information as possible is obtained from service 
                Log.Error(e, "Error obtaining trades from Power Service");
                throw;
            }
        }

        public async Task<IEnumerable<PowerPeriod>> ProcessForDateTimeAsync(DateTime dateTime, IEnumerable<PowerTrade> specificTrades = null)
        {
            var timeText = dateTime.ToString("dd/MM/yyyy HH:mm");
            Log.Information($"Extracting trades for {timeText}");
            var summatedPeriods = new List<PowerPeriod>();
            try
            {
                var trades = specificTrades ?? await GetTradesAsync(dateTime);
                summatedPeriods = SummatePeriods(trades, dateTime);

                var filename = Path.Combine(_configurationProvider.OutputFolder,
                    $"PowerPosition_{dateTime.ToString("yyyyMMdd")}_{dateTime.ToString("HHmm")}.csv");
                OutputToFile(summatedPeriods, filename);

                var tradeText = trades.Count() > 1 ? "trades" : "trade";
                Log.Information($"{trades.Count()} {tradeText} for {timeText} extracted and position saved to {filename}");
            }
            catch (Exception e)
            {
                if (e is PowerServiceException == false)
                {
                    Log.Error(e, "Error extracting trades");
                    throw;
                }
            }

            return summatedPeriods;
        }
    }
}