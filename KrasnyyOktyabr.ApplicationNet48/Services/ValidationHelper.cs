﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KrasnyyOktyabr.ApplicationNet48.Models.Configuration.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KrasnyyOktyabr.ApplicationNet48.Services;

public static class ValidationHelper
{
    /// <exception cref="ValidationException"></exception>
    public static void ValidateObject(object objectToValidate)
    {
        List<ValidationResult> validationResults = [];

        if (!Validator.TryValidateObject(objectToValidate, new ValidationContext(objectToValidate), validationResults, validateAllProperties: true))
        {
            throw new ValidationException(
                validationResult: validationResults[0],
                validatingAttribute: null,
                value: objectToValidate);
        }
    }

#nullable enable

    /// <remarks>
    /// Validates only outer properties, nested objects are ignored.
    /// </remarks>
    public static TSettings[]? GetAndValidateKafkaClientSettings<TSettings>(IConfiguration configuration, string section, ILogger logger)
    {
        TSettings[]? clientsSettings = configuration
            .GetSection(section)
            .Get<TSettings[]>();

        if (clientsSettings is null || clientsSettings.Length == 0)
        {
            return null;
        }

        try
        {
            foreach (TSettings clientSettings in clientsSettings)
            {
                if (clientSettings is null)
                {
                    continue;
                }

                ValidateObject(clientSettings);
            }

            return clientsSettings;
        }
        catch (ValidationException ex)
        {
            logger.LogError(ex, "Invalid configuration at '{Position}'",  section);
        }

        return null;
    }

    public static TSettings[]? GetAndValidateVApplicationKafkaClientSettings<TSettings>(IConfiguration configuration, string section, ILogger logger) where TSettings : AbstractVApplicationProducerSettings
    {
        TSettings[]? clientsSettings = GetAndValidateKafkaClientSettings<TSettings>(configuration, section, logger);

        if (clientsSettings is null || clientsSettings.Length == 0)
        {
            return null;
        }

        try
        {
            foreach (TSettings clientSettings in clientsSettings)
            {
                foreach (VApplicationObjectFilter objectFilter in clientSettings.ObjectFilters)
                {
                    ValidateObject(objectFilter);
                }
            }

            return clientsSettings;
        }
        catch (ValidationException ex)
        {
            logger.LogError(ex, "Invalid configuration at '{Position}'", section);
        }

        return null;
    }
}
