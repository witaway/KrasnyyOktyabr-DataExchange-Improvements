using System.Collections.Concurrent;
using System.Text;
using KrasnyyOktyabr.JsonTransform.Expressions;
using KrasnyyOktyabr.JsonTransform.Expressions.Creation;
using KrasnyyOktyabr.Scripting.Core;
using KrasnyyOktyabr.Scripting.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static KrasnyyOktyabr.JsonTransform.Helpers.JsonHelper;

namespace KrasnyyOktyabr.JsonTransform;

public sealed class ScriptingJsonTransformService(
    IJsonAbstractExpressionFactory factory,
    ILogger<ScriptingJsonTransformService> logger) : IScriptingService
{
    public static string ConsumerInstructionsPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Properties", "ConsumerInstructions");

    public static string InstructionsPropertyName => "instructions";

    public static string InputPropertyName => "input";

    private readonly ConcurrentDictionary<string, IExpression<Task>> _instructionNamesExpressions = [];

    public int ClearCachedExpressions()
    {
        int instructionsCount = _instructionNamesExpressions.Count;

        _instructionNamesExpressions.Clear();

        return instructionsCount;
    }

    /// <param name="outputStream">Is written synchronously.</param>
    public async ValueTask RunScriptArbitraryAsync(Stream scriptStream, Stream inputStream, Stream outputStream,
        CancellationToken cancellationToken)
    {
        // EP1. Load script JSON from stream
        using var scriptStreamReader = new StreamReader(
            scriptStream,
            Encoding.UTF8,
            true, 1024, true
        );

        JArray script = await JArray.LoadAsync(
            new JsonTextReader(scriptStreamReader),
            cancellationToken
        );
        
        // EP2. Load input data from stream
        using var inputStreamReader = new StreamReader(
            inputStream,
            Encoding.UTF8,
            true, 1024, true
        );

        JObject input = await JObject.LoadAsync(
            new JsonTextReader(inputStreamReader),
            cancellationToken
        );
        
        // EP3. Build expression
        IExpression<Task> expression = factory.Create<IExpression<Task>>(script);
        
        // EP4. Build context from input
        Context context = new(input);

        // EP5. Run script on context
        await expression.InterpretAsync(context, cancellationToken);

        // EP6. Format result
        JArray result = JArray.FromObject(context.OutputGet().Select(Unflatten));
        
        // JArray result = [];
        // foreach (JObject item in context.OutputGet())
        // {
        //     result.Add(Unflatten(item));
        // }

        StreamWriter writer = new(outputStream);
        JsonSerializer.CreateDefault().Serialize(writer, result);

        await writer.FlushAsync();
    }

    /// <exception cref="ArgumentNullException"></exception>
    public async ValueTask<List<JsonTransformMsSqlResult>> RunScriptOnConsumedMessageMsSqlAsync(
        string instructionName,
        string jsonObject,
        string tablePropertyName,
        CancellationToken cancellationToken = default)
    {
        List<JObject> jsonTransformResults = await RunJsonTransformOnConsumedMessageAsync(
            instructionName,
            jsonObject,
            cancellationToken);

        List<JsonTransformMsSqlResult> results = new(jsonTransformResults.Count);

        foreach (JObject jsonTransformResult in jsonTransformResults)
        {
            // Extract table name
            string tableName = jsonTransformResult[tablePropertyName]?.Value<string>() ??
                               throw new TablePropertyNotFoundException(tablePropertyName);
            jsonTransformResult.Remove(tablePropertyName);

            results.Add(new JsonTransformMsSqlResult(
                table: tableName,
                columnValues: jsonTransformResult.ToObject<Dictionary<string, dynamic>>()!
            ));
        }

        return results;
    }

    /// <exception cref="ArgumentNullException"></exception>
    public async ValueTask<List<string>> RunScriptOnConsumedMessageVApplicationAsync(
        string instructionName,
        string jsonObject,
        CancellationToken cancellationToken = default)
    {
        List<JObject> jsonTransformResults = await RunJsonTransformOnConsumedMessageAsync(
            instructionName,
            jsonObject,
            cancellationToken);

        List<string> results = new(jsonTransformResults.Count);

        foreach (JObject jsonTransformResult in jsonTransformResults)
        {
            results.Add(jsonTransformResult.ToString(Formatting.None));
        }

        return results;
    }

    /// <exception cref="ArgumentNullException"></exception>
    private async ValueTask<List<JObject>> RunJsonTransformOnConsumedMessageAsync(
        string instructionName,
        string objectJson,
        CancellationToken cancellationToken)
    {
        if (instructionName is null)
        {
            throw new ArgumentNullException(nameof(instructionName));
        }

        if (objectJson is null)
        {
            throw new ArgumentNullException(nameof(objectJson));
        }

        JObject input = ParseObjectJson(objectJson);

        IExpression<Task> expression = await GetExpressionAsync(instructionName);

        Context context = new(input);

        try
        {
            await expression.InterpretAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new JsonTransformException(instructionName, ex);
        }

        return context.OutputGet().Select(Unflatten).ToList();
    }

    /// <exception cref="ArgumentException"></exception>
    private static JObject ParseObjectJson(string objectJson)
    {
        try
        {
            return JObject.Parse(objectJson);
        }
        catch (JsonReaderException ex)
        {
            throw new ArgumentException("Failed to parse JSON", ex);
        }
    }

    private static void AddProperties(JObject jObject, Dictionary<string, object?> propertiesToAdd)
    {
        foreach (KeyValuePair<string, object?> property in propertiesToAdd)
        {
            jObject[property.Key] = JToken.FromObject(property.Value ?? JValue.CreateNull());
        }
    }

    private async ValueTask<IExpression<Task>> GetExpressionAsync(string instructionName)
    {
        if (_instructionNamesExpressions.TryGetValue(instructionName, out IExpression<Task>? cachedExpression))
        {
            return cachedExpression;
        }

        string instructionFilePath = Path.Combine(ConsumerInstructionsPath, instructionName);

        logger.LogTrace("{InstructionName} not found in cache, loading from '{FilePath}'", instructionName,
            instructionFilePath);

        JToken instructions = await LoadInstructionAsync(instructionFilePath);

        IExpression<Task> expression = factory.Create<IExpression<Task>>(instructions);

        _instructionNamesExpressions.TryAdd(instructionName, expression); // Race condition possible

        return expression;
    }

    private static async Task<JToken> LoadInstructionAsync(string filePath)
    {
        using StreamReader reader = File.OpenText(filePath);

        return await JToken.LoadAsync(new JsonTextReader(reader));
    }

    public class TablePropertyNotFoundException(string tablePropertyName)
        : Exception($"'{tablePropertyName}' property not found")
    {
    }

    public class JsonTransformException(string instructionName, Exception exception)
        : Exception($"At '{instructionName}'", exception)
    {
    }
}