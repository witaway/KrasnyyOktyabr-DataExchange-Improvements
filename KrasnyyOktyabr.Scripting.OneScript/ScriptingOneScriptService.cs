using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KrasnyyOktyabr.DataResolve;
using KrasnyyOktyabr.JsonTransform;
using KrasnyyOktyabr.Scripting.Core;
using KrasnyyOktyabr.Scripting.Core.Models;
using KrasnyyOktyabr.Scripting.OneScript.Engine;
using KrasnyyOktyabr.Scripting.OneScript.Logic.Api;
using KrasnyyOktyabr.Scripting.OneScript.Logic.ScriptedWorker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KrasnyyOktyabr.Scripting.OneScript;

public class ScriptingOneScriptService(
    IDataResolveService dataResolveService
) : IScriptingService
{
    public int ClearCachedExpressions()
    {
        throw new System.NotImplementedException();
    }

    public async ValueTask RunScriptArbitraryAsync(
        Stream scriptStream,
        Stream inputStream,
        Stream outputStream,
        CancellationToken cancellationToken
    )
    {
        using var outputStreamWriter = new StreamWriter(outputStream, Encoding.UTF8, 512, true);

        var host = new DefaultAppHost((message, messageStatusEnum) => { });

        var engine = EngineProvider.CreateEngine(host, dataResolveService);
        engine.Initialize();

        var worker = Worker.CreateFromScriptStream(engine, scriptStream);

        var inputJsonData = new JsonData(inputStream, cannotBeArray: true);
        var result = worker.ProccessScript(inputJsonData);
        
        await outputStreamWriter.WriteAsync(result.Serialize());
    }

    public async ValueTask<List<JsonTransformMsSqlResult>> RunScriptOnConsumedMessageMsSqlAsync(
        string instructionName,
        string message,
        string tablePropertyName,
        CancellationToken cancellationToken
    )
    {
        var inputJsonData = new JsonData(message, cannotBeArray: true);
        var result = await RunScriptInternal(instructionName, inputJsonData);

        if (result.Root is not JArray resultJArray)
        {
            throw new Exception("Output of MsSQL consumer must return JArray.");
        }
        
        List<JsonTransformMsSqlResult> results = new(resultJArray.Count);

        foreach (var jToken in resultJArray)
        {
            var jsonTransformResult = (JObject)jToken;
            
            // Extract table name
            string tableName = jsonTransformResult[tablePropertyName]?.Value<string>() ??
                               throw new ScriptingJsonTransformService.TablePropertyNotFoundException(tablePropertyName);
            jsonTransformResult.Remove(tablePropertyName);

            results.Add(new JsonTransformMsSqlResult(
                table: tableName,
                columnValues: jsonTransformResult.ToObject<Dictionary<string, dynamic>>()!
            ));
        }

        return results;
    }

    public async ValueTask<List<string>> RunScriptOnConsumedMessageVApplicationAsync(
        string instructionName,
        string message,
        CancellationToken cancellationToken
    )
    {
        var inputJsonData = new JsonData(message, cannotBeArray: true);
        var resultJsonData = await RunScriptInternal(instructionName, inputJsonData);
        
        var resultArray = new List<string>();
        
        if (resultJsonData.Root is not JArray resultJArray)
        {
            throw new Exception("Output of MsSQL consumer must return JArray.");
        }

        foreach (var jToken in resultJArray)
        {
            resultArray.Add(jToken.ToString(Formatting.None));
        }
        
        return resultArray;
    }

    private async Task<JsonData> RunScriptInternal(string instructionName, JsonData input)
    {
        var host = new DefaultAppHost((message, messageStatusEnum) => { });

        var engine = EngineProvider.CreateEngine(host, dataResolveService);
        engine.Initialize();

        var worker = Worker.CreateFromFile(engine, instructionName);
        
        return worker.ProccessScript(input);
    }
}