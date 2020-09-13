using Autofac;
using Serilog;
using System;

namespace DevTest.Trades.Common.Helpers
{
    public static class AutoFacHelper
    {
        public static IContainer SetupContainer()
        {
            var builder = new ContainerBuilder();
            var logPath = (AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log.log");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            builder.RegisterAssemblyModules(assemblies);
            builder.Register<ILogger>((c, p) =>
            {
                return new LoggerConfiguration()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                    .WriteTo.Console()
                    .CreateLogger();
            }).SingleInstance();

            var result = builder.Build();
            return result;
        }
    }
}