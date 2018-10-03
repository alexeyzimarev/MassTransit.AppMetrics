using App.Metrics;
using GreenPipes;

namespace MassTransit.AppMetrics
{
    public static class AppMetricsMiddlewareConfigurationExtensions
    {
        public static void UseAppMetrics<T>(this IPipeConfigurator<T> configurator, IMetrics metrics)
            where T : class, ConsumeContext
            => configurator.AddPipeSpecification(new AppMetricsSpecification<T>(metrics));
    }
}