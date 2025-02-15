﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KrasnyyOktyabr.ApplicationNet48.Services.Kafka;

public static class V77ApplicationHelper
{
    public static string DefaultProducerErtRelativePath => Path.Combine("ExtForms", "EDO", "Test", "GetObjectJson.ert");

    public static string DefaultConsumerErtRelativePath => Path.Combine("ExtForms", "EDO", "Test", "SaveObject.ert");

    public static char ObjectFilterValuesSeparator => ':';

    public static int ObjectFilterDefaultDepth => 1;

    public static string TransactionTypePropertyName => "ТипТранзакции";

    public static string ObjectDatePropertyName => "ДатаДокИзЛогов";

    /// <exception cref="OperationCanceledException"></exception>
    public static async ValueTask WaitRdSessionsAllowed(IWmiService wmiService, CancellationToken cancellationToken = default, ILogger logger = null)
    {
        try
        {
            bool? areRdSessionsAllowed = wmiService.AreRdSessionsAllowed();

            while (areRdSessionsAllowed == false)
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger?.LogTrace("Wait until RDP is allowed");

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                areRdSessionsAllowed = wmiService.AreRdSessionsAllowed();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to check is RDP allowed");
        }
    }

    public class FailedToGetObjectException : Exception
    {
        internal FailedToGetObjectException(string objectId)
            : base($"Failed to get object with id '{objectId}'")
        {
        }
    }
}
