using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Meter;
using App.Metrics.Timer;
using GreenPipes;

namespace MassTransit.AppMetrics
{
    public class AppMetricsFilter<T> : IFilter<T>
        where T : class, ConsumeContext
    {
        private readonly IMetrics _metrics;
        private readonly MeterOptions _globalMeter;
        private readonly MeterOptions _globalErrorsMeter;
        private readonly Dictionary<string, MeterOptions> _consumerMeters;
        private readonly Dictionary<string, MeterOptions> _consumerErrorsMeters;
        private readonly Dictionary<string, TimerOptions> _consumerTimers;

        public AppMetricsFilter(IMetrics metrics)
        {
            _metrics = metrics;
            _globalMeter = new MeterOptions
            {
                Name = "messages",
                MeasurementUnit = Unit.Events
            };
            _globalErrorsMeter = new MeterOptions
            {
                Name = "errors",
                MeasurementUnit = Unit.Errors
            };
            _consumerMeters = new Dictionary<string, MeterOptions>();
            _consumerErrorsMeters = new Dictionary<string, MeterOptions>();
            _consumerTimers = new Dictionary<string, TimerOptions>();
        }

        public async Task Send(T context, IPipe<T> next)
        {
            var messageType = context.SupportedMessageTypes.FirstOrDefault() ?? "unknown";
            var shortTypeName = messageType.Split('.').Last();

            _metrics.Measure.Meter.Mark(_globalMeter);
            _metrics.Measure.Meter.Mark(GetOrAddMeter(shortTypeName, _consumerMeters));

            try
            {
                using (_metrics.Measure.Timer.Time(GetOrAddTimer(shortTypeName, _consumerTimers)))
                {
                    await next.Send(context).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                _metrics.Measure.Meter.Mark(_globalErrorsMeter);
                _metrics.Measure.Meter.Mark(GetOrAddMeter(shortTypeName, _consumerErrorsMeters));

                throw;
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("AppMetricsFilter");

        private static MeterOptions GetOrAddMeter(string type, IDictionary<string, MeterOptions> meters)
        {
            if (meters.ContainsKey(type)) return meters[type];

            var meter = new MeterOptions
            {
                Name = $"{type}_messages",
                MeasurementUnit = Unit.Events
            };
            meters[type] = meter;
            return meter;
        }

        private static TimerOptions GetOrAddTimer(string type, IDictionary<string, TimerOptions> timers)
        {
            if (timers.ContainsKey(type)) return timers[type];

            var timer = new TimerOptions
            {
                Name = $"{type}_time",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };
            timers[type] = timer;
            return timer;
        }
    }
}