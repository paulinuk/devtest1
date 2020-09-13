using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Catel.Linq;
using DevTest.Trades.Services;
using DevTest.Trades.Services.Interfaces;
using FluentAssertions;
using Moq;
using Quartz.Impl;
using Serilog;
using Services;
using Xunit;

namespace DevTest.Trades.Tests
{
    public class TradeExtractorServiceTests
    {
        private ITradeExtractorService _service;
        private IConfigurationProvider _configurationProvider;

        public TradeExtractorServiceTests()
        {
            var scheduler = new StdSchedulerFactory().GetScheduler();
            var mockLogger = new Mock<ILogger>();

            _configurationProvider = new ConfigurationProvider(mockLogger.Object, @"c:\temp\testoutputs");

            _service = new TradeExtractorService(scheduler, _configurationProvider, mockLogger.Object);
        }

        private List<PowerTrade> SampleTrades(DateTime datetime)
        {
            var trades = new List<PowerTrade>()
            {
                PowerTrade.Create(datetime.AddMinutes(1), 24),
                PowerTrade.Create(datetime.AddMinutes(2), 24)
            };

            trades[0].Periods[0].Volume = 100;
            trades[1].Periods[0].Volume = 50;

            return trades;
        }

        [Fact]
        public async void ShouldSummateCorrectly()
        {
            var datetime = Convert.ToDateTime("2015-04-01").ToUniversalTime();
            var trades = SampleTrades(datetime);

            var summatedPeriods = await _service.ProcessForDateTimeAsync(datetime, trades).ConfigureAwait(false);
            summatedPeriods.Should().NotBeEmpty();

            var list = summatedPeriods.ToList();
            list[0].Volume.Should().Be(150);
        }

        //If no trades - raise an error - make sure its reported as an exception.
        [Fact]
        public async void NoDataShouldThrowException()
        {
            var datetime = Convert.ToDateTime("2015-04-01").ToUniversalTime();
            var trades = new List<PowerTrade>();

            Func<Task> act = async () => await _service.ProcessForDateTimeAsync(datetime, trades).ConfigureAwait(false);
            act.Should().Throw<DataException>().WithMessage("No trades were found for 20150331 23:00");
        }

        //Verify that the file is created as expected and the values are correct
        [Fact]
        public async void OutputFileShouldBeCorrect()
        {
            var datetime = Convert.ToDateTime("2015-04-01 13:45").ToUniversalTime();
            var trades = SampleTrades(datetime);
            await _service.ProcessForDateTimeAsync(datetime, trades).ConfigureAwait(false);

            var filename = Path.Combine(_configurationProvider.OutputFolder,
                $"PowerPosition_{datetime.ToString("yyyyMMdd")}_{datetime.ToString("HHmm")}.csv");
            File.Exists(filename).Should().Be(true);

            var contents = File.ReadAllLines(filename).ToList();
            contents.Count.Should().Be(25);
            contents[contents.Count - 1].Should().StartWith("22:00");
            contents[0].Should().Equals("Period,Volume");
        }
    }
}