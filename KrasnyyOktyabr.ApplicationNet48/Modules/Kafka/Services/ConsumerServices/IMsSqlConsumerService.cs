using System;
using KrasnyyOktyabr.ApplicationNet48.Models.Kafka;
using KrasnyyOktyabr.ApplicationNet48.Services;

namespace KrasnyyOktyabr.ApplicationNet48.Modules.Kafka.Services.ConsumerServices;

public interface IMsSqlConsumerService : IRestartableHostedService<IStatusContainer<MsSqlConsumerStatus>>, IAsyncDisposable
{
}
