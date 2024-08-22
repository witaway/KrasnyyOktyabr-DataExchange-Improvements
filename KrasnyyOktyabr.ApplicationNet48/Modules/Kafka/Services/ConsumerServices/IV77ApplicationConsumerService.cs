using System;
using KrasnyyOktyabr.ApplicationNet48.Models.Kafka;
using KrasnyyOktyabr.ApplicationNet48.Services;

namespace KrasnyyOktyabr.ApplicationNet48.Modules.Kafka.Services.ConsumerServices;

public interface IV77ApplicationConsumerService : IRestartableHostedService<IStatusContainer<V77ApplicationConsumerStatus>>, IAsyncDisposable
{
}
