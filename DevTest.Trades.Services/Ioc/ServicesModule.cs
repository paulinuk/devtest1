namespace DevTest.Trades.Services.Ioc
{
    using Autofac;
    using DevTest.Trades.Services.Interfaces;
    using Quartz;
    using Quartz.Impl;

    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TradeExtractorService>().As<ITradeExtractorService>();
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();
            builder.Register(x => new StdSchedulerFactory().GetScheduler()).As<IScheduler>();
        }
    }
}