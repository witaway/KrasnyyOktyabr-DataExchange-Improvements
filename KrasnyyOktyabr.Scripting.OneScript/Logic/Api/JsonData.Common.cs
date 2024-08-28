using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[ContextClass("Данные", "Data")]
public partial class JsonData : AutoContext<JsonData>
{
    private static readonly List<DataType> _oscriptPlainDataTypes =
    [
        DataType.Boolean,
        DataType.Date,
        DataType.Number,
        DataType.String
    ];

    public JContainer Root { get; }

    public JsonDataTypeEnum? CheckedTypeForIndexOperatorAccess = null;

    public JsonData(
        Stream inputStream,
        bool cannotBeArray = false,
        JsonDataTypeEnum? checkedTypeForIndexOperatorAccess = null
    )
    {
        using var inputStreamReader = new StreamReader(
            inputStream,
            Encoding.UTF8,
            true, 1024, true
        );
        
        using var reader = new JsonTextReader(inputStreamReader);
        reader.FloatParseHandling = FloatParseHandling.Decimal;
        
        var loadSettings = new JsonLoadSettings();

        try
        {
            var inputJson = JToken.Load(reader, loadSettings) as JContainer;
            if (inputJson is null) throw new JsonReaderException();
            if (cannotBeArray && inputJson is JArray) throw new JsonReaderException();

            Root = inputJson;
        }
        catch (JsonReaderException ex)
        {
            throw new RuntimeException(
                "Невозможно создать Данные. Переданный объект содержит синтаксические ошибки?", ex
            );
        }
        catch (CannotCastToJContainerException ex)
        {
            throw new RuntimeException(
                "Невозможно создать Данные. Переданный объект не является JContainer?", ex
            );
        }
        catch (CannotBeArrayException ex)
        {
            throw new RuntimeException(
                "Невозможно создать Данные. Переданный объект не может быть массивом", ex
            );
        }

        CheckedTypeForIndexOperatorAccess = checkedTypeForIndexOperatorAccess;
    }
    
    public JsonData(
        string input,
        bool cannotBeArray = false,
        JsonDataTypeEnum? checkedTypeForIndexOperatorAccess = null
    )
    {
        try
        {
            if (JToken.Parse(input) is not JContainer inputJson) throw new JsonReaderException();
            if (cannotBeArray && inputJson is JArray) throw new JsonReaderException();

            Root = inputJson;
        }
        catch (JsonReaderException ex)
        {
            throw new RuntimeException(
                "Невозможно создать Данные. Переданный объект содержит синтаксические ошибки?", ex
            );
        }
        catch (CannotCastToJContainerException ex)
        {
            throw new RuntimeException(
                "Невозможно создать Данные. Переданный объект не является JContainer?", ex
            );
        }
        catch (CannotBeArrayException ex)
        {
            throw new RuntimeException(
                "Невозможно создать Данные. Переданный объект не может быть массивом", ex
            );
        }

        CheckedTypeForIndexOperatorAccess = checkedTypeForIndexOperatorAccess;
    }

    public JsonData(
        JContainer root,
        bool cannotBeArray = false,
        JsonDataTypeEnum? checkedTypeForIndexOperatorAccess = null
    )
    {
        if (root.Type == JTokenType.Array && cannotBeArray)
        {
            throw new RuntimeException("Невозможно создать Данные. Переданный объект не может быть массивом");
        }

        Root = root;
        CheckedTypeForIndexOperatorAccess = checkedTypeForIndexOperatorAccess;
    }

    [ScriptConstructor]
    public static JsonData Constructor(string json)
    {
        return new JsonData(JContainer.Parse(json) as JContainer);
    }

    [ContextMethod("Сериализовать", "Serialize")]
    public string Serialize()
    {
        // Todo: There's should be a into Stream implementation too 
        return Root.ToString();
    }

    protected bool CheckJsonType(JToken value, JsonDataTypeEnum checkedType)
    {
        return value.Type switch
        {
            JTokenType.Boolean => checkedType == JsonDataTypeEnum.Boolean,
            JTokenType.String => checkedType == JsonDataTypeEnum.String,
            JTokenType.Date => checkedType == JsonDataTypeEnum.Date,
            JTokenType.Integer => checkedType == JsonDataTypeEnum.Number,
            JTokenType.Float => checkedType == JsonDataTypeEnum.Number,
            JTokenType.Guid => checkedType == JsonDataTypeEnum.String,
            JTokenType.Uri => checkedType == JsonDataTypeEnum.String,

            JTokenType.Object => checkedType == JsonDataTypeEnum.Object,
            JTokenType.Array => checkedType == JsonDataTypeEnum.Array,
        };
    }

    protected static JToken IntoJsonType(IValue value)
    {
        // For plain types
        if (_oscriptPlainDataTypes.Contains(value.DataType))
        {
            return JToken.FromObject(value.DataType switch
            {
                DataType.Boolean => value.AsBoolean(),
                DataType.Date => value.AsDate(),
                DataType.Number => value.AsNumber(),
                DataType.String => value.AsString()
            });
        }

        // For JsonData instances
        if (value.DataType == DataType.Object && value is JsonData jsonData)
        {
            return jsonData.Root;
        }

        throw new RuntimeException("Невозможно привести OneScript тип к Json типу");
    }

    protected static IValue IntoOneScriptType(JToken token, bool unwrapIfArray = false)
    {
        try
        {
            return token.Type switch
            {
                JTokenType.Boolean =>
                    ValueFactory.Create(token.Value<bool>()),

                JTokenType.Float =>
                    ValueFactory.Create(token.Value<decimal>()),

                JTokenType.Integer =>
                    ValueFactory.Create(token.Value<int>()),

                JTokenType.String =>
                    ValueFactory.Create(token.Value<string>()),

                JTokenType.Date =>
                    ValueFactory.Create(token.Value<DateTime>()),

                JTokenType.Object or JTokenType.Array =>
                    ContextValuesMarshaller.ConvertDynamicValue(
                        new JsonData(token as JContainer)
                    )
            };
        }
        catch (Exception ex)
        {
            throw new RuntimeException("Невозможно привести JSON ни к одному известному OneScript типу. Баг?", ex);
        }
    }

    public class CannotCastToJContainerException() : Exception
    {
    }

    public class CannotBeArrayException : Exception
    {
    }
}