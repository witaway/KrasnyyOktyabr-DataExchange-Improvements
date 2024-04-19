﻿using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using KrasnyyOktyabr.ComV77Application;
using KrasnyyOktyabr.ComV77Application.Contracts.Configuration;
using KrasnyyOktyabr.JsonTransform.Expressions.DataResolve;
using Newtonsoft.Json.Linq;
using static KrasnyyOktyabr.JsonTransform.Expressions.DataResolve.IDataResolveService;

namespace KrasnyyOktyabr.Application.Services.DataResolve;

public class DataResolveService : IDataResolveService
{
    public static string DefaultErtRelativePathWithoutName => Path.Combine("EXTFORMS", "EDO", "Test");

    private readonly Dictionary<string, Func<Dictionary<string, object?>, IDataResolver>> _resolverFactories;

    public DataResolveService(HttpClient httpClient, IMsSqlService msSqlService, IComV77ApplicationConnectionFactory connectionFactory)
    {
        _resolverFactories = new()
        {
            { nameof(HttpDataResolver), args => CreateHttpDataResolver(args, httpClient) },
            { nameof(MsSqlSingleValueDataResolver), args => CreateMsSqlSingleValueDataResolver(args, msSqlService) },
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _resolverFactories.Add(
                key: nameof(ComV77ApplicationResolver),
                value: args =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return CreateComV77ApplicationResolver(args, connectionFactory);
                    }

                    throw new NotSupportedException();
                });
        }
    }

    public async ValueTask<object?> ResolveAsync(string resolverName, Dictionary<string, object?> args, CancellationToken cancellationToken)
    {
        if (!_resolverFactories.TryGetValue(resolverName, out Func<Dictionary<string, object?>, IDataResolver>? resolverFactory))
        {
            throw new ResolverNotFoundException(resolverName);
        }

        return await resolverFactory(args).ResolveAsync(cancellationToken);
    }

    /// <exception cref="ArgumentException"></exception>
    private static HttpDataResolver CreateHttpDataResolver(Dictionary<string, object?> args, HttpClient httpClient)
    {
        string url = GetRequired<string>(args, "url");
        string method = GetRequired<string>(args, "method");

        string? contentType = GetOptional<string?>(args, "contentType", null);
        string? credentialsUsername = GetOptional<string?>(args, "username", null);
        string? credentialsPassword = GetOptional<string?>(args, "password", null);
        string? body = GetOptional<string?>(args, "body", null);

        HttpRequestMessage request = new(new HttpMethod(method), url)
        {
            Content = GetHttpContent(contentType, body),
        };

        if (credentialsUsername != null)
        {
            request.Headers.Authorization = GetAuthenticationHeaderValue(credentialsUsername, credentialsPassword);
        }

        return new(httpClient, request);
    }

    /// <exception cref="ArgumentException"></exception>
    private static MsSqlSingleValueDataResolver CreateMsSqlSingleValueDataResolver(Dictionary<string, object?> args, IMsSqlService msSqlService)
    {
        string connectionString = GetRequired<string>(args, "connectionString");
        string query = GetRequired<string>(args, "query");

        return new(msSqlService, connectionString, query);
    }

    /// <exception cref="ArgumentException"></exception>
    [SupportedOSPlatform("windows")]
    private static ComV77ApplicationResolver CreateComV77ApplicationResolver(Dictionary<string, object?> args, IComV77ApplicationConnectionFactory connectionFactory)
    {
        string infobasePath = GetRequired<string>(args, "infobasePath");
        string username = GetRequired<string>(args, "username");
        string password = GetRequired<string>(args, "password");
        string ertName = GetRequired<string>(args, "ertName");

        Dictionary<string, object?>? context = GetOptional<Dictionary<string, object?>?>(args, "formParams", null);
        string? resultName = GetOptional<string?>(args, "resultName", null);

        ConnectionProperties connectionProperties = new()
        {
            InfobasePath = infobasePath,
            Username = username,
            Password = password,
        };

        return new(connectionFactory, connectionProperties, Path.Combine(DefaultErtRelativePathWithoutName, ertName), context, resultName);
    }

    private static T GetRequired<T>(Dictionary<string, object?> args, string name)
    {
        if (!args.TryGetValue(name, out object? value))
        {
            throw new ArgumentException($"No '{name}' parameter");
        }

        if (value is T checkedValue)
        {
            return checkedValue;
        }

        if (value != null)
        {
            if (TryConvertThroughJToken(value, out T? converted))
            {
                return converted!;
            }
        }

        throw new ArgumentException($"Parameter '{name}' type is invalid ({name.GetType().Name} instead of {typeof(T).Name})");
    }

    private static T GetOptional<T>(Dictionary<string, object?> args, string name, T defaultValue)
    {
        bool isPresent = args.TryGetValue(name, out object? value);

        if (!isPresent)
        {
            return defaultValue;
        }

        if (value is T checkedValue)
        {
            return checkedValue;
        }

        if (value != null)
        {
            if (TryConvertThroughJToken(value, out T? converted))
            {
                return converted!;
            }
        }

        return defaultValue;
    }

    private static StringContent? GetHttpContent(string? contentType, string? body)
    {
        if (body == null)
        {
            return null;
        }

        return contentType == null
            ? new StringContent(body)
            : new StringContent(body, Encoding.UTF8, contentType);
    }

    private static AuthenticationHeaderValue GetAuthenticationHeaderValue(string username, string? password)
    {
        string credential = $"{username}:{password}";
        string encodedCredential = Convert.ToBase64String(Encoding.UTF8.GetBytes(credential));
        return new AuthenticationHeaderValue("Basic", encodedCredential);
    }

    private static bool TryConvertThroughJToken<T>(object value, out T? converted)
    {
        converted = default;

        T? convertedValue = JToken.FromObject(value).ToObject<T>();

        if (convertedValue != null)
        {
            converted = convertedValue;

            return true;
        }

        return false;
    }
}
