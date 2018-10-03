using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using GreenPipes;

namespace MassTransit.AppMetrics
{
    public class AppMetricsSpecification<T> : IPipeSpecification<T>
        where T : class, ConsumeContext
    {
        private readonly IMetrics _metricsRoot;

        public AppMetricsSpecification(IMetrics metricsRoot) => _metricsRoot = metricsRoot;

        public void Apply(IPipeBuilder<T> builder)
            => builder.AddFilter(new AppMetricsFilter<T>(_metricsRoot));

        public IEnumerable<ValidationResult> Validate() => Enumerable.Empty<ValidationResult>();
    }
}