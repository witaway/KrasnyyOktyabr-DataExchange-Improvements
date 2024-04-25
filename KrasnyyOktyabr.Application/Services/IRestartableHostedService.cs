﻿using KrasnyyOktyabr.Application.Contracts.Kafka;

namespace KrasnyyOktyabr.Application.Services;

public interface IRestartableHostedService : IHostedService, IRestartable
{
    int ManagedInstancesCount { get; }
}

/// <summary>
/// <see cref="IRestartableHostedService"/> with <see cref="Status"/> property.
/// </summary>
public interface IRestartableHostedService<out TStatus> : IRestartableHostedService where TStatus : IStatusContainer<AbstractStatus>
{
    TStatus Status { get; }
}
