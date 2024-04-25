﻿using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using Confluent.Kafka;
using KrasnyyOktyabr.Application.Contracts.Configuration.Kafka;
using KrasnyyOktyabr.Application.Logging;
namespace KrasnyyOktyabr.Application.Services.Kafka;

public sealed partial class KafkaService : IKafkaService
{
    private readonly ILogger<KafkaService> _logger;

    private readonly IConfiguration _configuration;

    private readonly ITransliterationService _transliterationService;

    private KafkaSettings? _settings;

    public KafkaService(
        IConfiguration configuration,
        ITransliterationService transliterationService,
        ILogger<KafkaService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _transliterationService = transliterationService;

        LoadKafkaSettings();
    }

    public void Restart()
    {
        LoadKafkaSettings();
    }

    /// <exception cref="NoKafkaSettingsException"></exception>
    public IProducer<TKey, TValue> GetProducer<TKey, TValue>()
    {
        if (_settings == null)
        {
            throw new NoKafkaSettingsException();
        }

        ProducerConfig config = new()
        {
            BootstrapServers = _settings.Socket,
            MessageMaxBytes = _settings.MessageMaxBytes,
        };

        return new ProducerBuilder<TKey, TValue>(config).Build();
    }

    /// <exception cref="NoKafkaSettingsException"></exception>
    public IConsumer<TKey, TValue> GetConsumer<TKey, TValue>(IEnumerable<string> topics, string consumerGroup)
    {
        if (_settings == null)
        {
            throw new NoKafkaSettingsException();
        }

        ConsumerConfig config = new()
        {
            GroupId = consumerGroup,
            BootstrapServers = _settings.Socket,
            EnableAutoCommit = false,
            MessageMaxBytes = _settings.MessageMaxBytes,
            MaxPollIntervalMs = _settings.MaxPollIntervalMs,
            AutoOffsetReset = AutoOffsetReset.Latest
        };

        IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(config).Build();

        consumer.Subscribe(topics);

        return consumer;
    }

    /// <summary>Updates <see cref="_settings"/>.</summary>
    /// <remarks>
    /// <see cref="_configuration"/> and <see cref="_logger"/> need to be initialized.
    /// </remarks>
    private void LoadKafkaSettings()
    {
        _settings = _configuration
                .GetSection(KafkaSettings.Position)
                .Get<KafkaSettings>();

        if (_settings == null)
        {
            _logger.ConfigurationNotFound();

            return;
        }

        try
        {
            ValidationHelper.ValidateObject(_settings);
        }
        catch (ValidationException ex)
        {
            _logger.InvalidConfiguration(ex, KafkaSettings.Position);
        }
    }

    public string BuildTopicName(params string[] names)
    {
        StringBuilder stringBuilder = new();

        for (int i = 0; i < names.Length; i++)
        {
            if (i != 0)
            {
                stringBuilder.Append('_');
            }

            stringBuilder.Append(names[i]);
        }

        return _transliterationService.TransliterateToLatin(stringBuilder.ToString());
    }

    public string ExtractConsumerGroupNameFromConnectionString(string connectionString)
    {
        return ConsumerGroupFromConnectionStringRegex()
            .Match(connectionString)
            .Groups[1]
            .Value;
    }

    public class NoKafkaSettingsException : Exception { }

    [GeneratedRegex(@"Database=(.+?);", RegexOptions.IgnoreCase, "ru-RU")]
    private static partial Regex ConsumerGroupFromConnectionStringRegex();
}
