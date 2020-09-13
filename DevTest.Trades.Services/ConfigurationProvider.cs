using Serilog;
using System;
using System.Configuration;
using System.IO;

namespace DevTest.Trades.Services
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string OutputFolder { get; set; }
        public int Interval { get; set; }
        public ConfigurationProvider(ILogger log, string outputFolder = null)
        {
            OutputFolder = outputFolder ?? ConfigurationManager.AppSettings["OutputFolder"];
            if (Directory.Exists(OutputFolder) == false)
            {
                Directory.CreateDirectory(OutputFolder);
            }

            var intervalSetting = ConfigurationManager.AppSettings["Interval"];
            Interval = intervalSetting != null ? int.Parse(intervalSetting) : 1;

            log.Information($"Output Folder: {OutputFolder}");
            var minutesText = Interval > 1 ? "minutes" : "minute";
            log.Information($"Extraction Interval: {Interval} {minutesText}");

            var logFolder = (AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log.log");
            Console.WriteLine($"Log Location: {logFolder}");
        }
    }
}