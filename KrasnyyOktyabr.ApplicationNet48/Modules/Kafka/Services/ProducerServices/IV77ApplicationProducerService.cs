using System;
using KrasnyyOktyabr.ApplicationNet48.Models.Kafka;
using KrasnyyOktyabr.ApplicationNet48.Services;

namespace KrasnyyOktyabr.ApplicationNet48.Modules.Kafka.Services.ProducerServices;

public interface IV77ApplicationProducerService : IRestartableHostedService<IStatusContainer<V77ApplicationProducerStatus>>, IAsyncDisposable
{
}
