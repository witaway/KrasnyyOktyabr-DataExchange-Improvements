using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KrasnyyOktyabr.Scripting.Core;
using KrasnyyOktyabr.Scripting.Core.Models;
using MyService.BusinessLogic.ScriptedWorker;
using MyService.OneScriptBridge;
using ScriptEngine.HostedScript.Library;

namespace KrasnyyOktyabr.Scripting.OneScript;

public class ScriptingOneScriptService : IScriptingService
{
    public int ClearCachedExpressions()
    {
        throw new System.NotImplementedException();
    }

    public async ValueTask RunJsonTransformAsync(Stream inputStream, Stream outputStream, CancellationToken cancellationToken)
    {
        using var inputStreamReader = new StreamReader(inputStream, Encoding.UTF8, true, 512, true);
        using var outputStreamWriter = new StreamWriter(outputStream, Encoding.UTF8, 512, true);
        
        // EP 1. Configure application host and input/output capabilities.
        var host = new DefaultAppHost((message, messageStatusEnum) =>
        {
            // Here ReSharper assumes captured variable outputStreamWriter
            // Could be used after disposal. It's not true in the case.
            // ReSharper disable AccessToDisposedClosure
            outputStreamWriter.WriteLine(message);
            outputStreamWriter.FlushAsync();
            // ReSharper restore AccessToDisposedClosure
        });
        
        // EP 2. Initialize scripting engine with given host
        var engine = EngineProvider.CreateEngine(host);
        engine.Initialize();

        // EP 3. Initialize worker
        var worker = Worker.CreateFromStream(engine, inputStream);
        
        // EP 4. Run worker
        var success = worker.ProccessWeather();
        
        // EP 5. Check success
        if (success)
            await outputStreamWriter.WriteLineAsync("\nРЕЗУЛЬТАТ ВЫПОЛНЕНИЯ: УСПЕХ!");
        else 
            await outputStreamWriter.WriteLineAsync("\\nРЕЗУЛЬТАТ ВЫПОЛНЕНИЯ: НЕ УСПЕХ :(");
    }

    public ValueTask<List<JsonTransformMsSqlResult>> RunJsonTransformOnConsumedMessageMsSqlAsync(string instructionName, string message, string tablePropertyName,
        CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public ValueTask<List<string>> RunJsonTransformOnConsumedMessageVApplicationAsync(string instructionName, string message,
        CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}