#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KrasnyyOktyabr.ApplicationNet48.Modules.Scripting;

public interface IScriptingService
{
    public readonly struct JsonTransformMsSqlResult(string table, Dictionary<string, dynamic> columnValues)
    {
        public string Table { get; } = table;

        public Dictionary<string, dynamic> ColumnValues { get; } = columnValues;
    }

    int ClearCachedExpressions();

    /// <param name="inputStream">
    /// Must contain JSON: <c>"{'instructions': ... ,'input': { ... } }"</c>
    /// </param>
    /// <remarks>
    /// <paramref name="outputStream"/> may be written synchronously.
    /// </remarks>
    /// <exception cref="Exception"></exception>
    ValueTask RunJsonTransformAsync(Stream inputStream, Stream outputStream, CancellationToken cancellationToken);

    /// <exception cref="Exception"></exception>
    ValueTask<List<JsonTransformMsSqlResult>> RunJsonTransformOnConsumedMessageMsSqlAsync(
        string instructionName,
        string message,
        string tablePropertyName,
        CancellationToken cancellationToken);

    /// <exception cref="Exception"></exception>
    ValueTask<List<string>> RunJsonTransformOnConsumedMessageVApplicationAsync(
        string instructionName,
        string message,
        CancellationToken cancellationToken);
}
