#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KrasnyyOktyabr.Scripting.Core.Models;

namespace KrasnyyOktyabr.Scripting.Core;

public interface IScriptingService
{
    int ClearCachedExpressions();

    /// <param name="inputStream">
    /// Must contain JSON: <c>"{'instructions': ... ,'input': { ... } }"</c>
    /// </param>
    /// <remarks>
    /// <paramref name="outputStream"/> may be written synchronously.
    /// </remarks>
    /// <exception cref="Exception"></exception>
    ValueTask RunScriptArbitraryAsync(
        Stream scriptStream,
        Stream inputStream,
        Stream outputStream,
        CancellationToken cancellationToken
    );

    /// <exception cref="Exception"></exception>
    ValueTask<List<JsonTransformMsSqlResult>> RunScriptOnConsumedMessageMsSqlAsync(
        string instructionName,
        string message,
        string tablePropertyName,
        CancellationToken cancellationToken
    );

    /// <exception cref="Exception"></exception>
    ValueTask<List<string>> RunScriptOnConsumedMessageVApplicationAsync(
        string instructionName,
        string message,
        CancellationToken cancellationToken
    );
}