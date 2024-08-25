using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[ContextClass("Данные", "Data")]
public class JsonData : AutoContext<JsonData>
{
    private JContainer _root;
    private JsonDataTypeEnum? _checkedTypeForIndexOperatorAccess = null;

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

            _root = inputJson;
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

        _checkedTypeForIndexOperatorAccess = checkedTypeForIndexOperatorAccess;
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

        _root = root;
        _checkedTypeForIndexOperatorAccess = checkedTypeForIndexOperatorAccess;
    }

    public override IValue GetIndexedValue(IValue index)
    {
        return index.DataType switch
        {
            DataType.String => GetSingleValueByPath(
                index.AsString(),
                _checkedTypeForIndexOperatorAccess
            ),

            DataType.Number => GetSingleValueByIndex(
                (int)index.AsNumber(),
                _checkedTypeForIndexOperatorAccess
            ),

            DataType.Enumeration => new JsonData(
                _root,
                false,
                ContextValuesMarshaller.CastToCLRObject<JsonDataTypeEnum>(index)
            ),

            _ => base.GetIndexedValue(index)
        };
    }

    [ContextMethod("Получить", "Get")]
    public IValue GetSingleValueByPath(IValue pathOrIndex, JsonDataTypeEnum? checkedType = null)
    {
        switch (pathOrIndex.DataType)
        {
            case DataType.String:
            {
                var path = pathOrIndex.AsString();
                return GetSingleValueByPath(path, checkedType);
            }
            case DataType.Number:
            {
                var index = (int)pathOrIndex.AsNumber();
                return GetSingleValueByIndex(index, checkedType);
            }
            default:
                throw new RuntimeException("Данные.Получить ожидает получить строку или число как первый аргумент");
        }
    }

    private IValue GetSingleValueByPath(string path, JsonDataTypeEnum? checkedType = null)
    {
        var jResult = _root.SelectToken(path);

        if (jResult is null)
        {
            throw new RuntimeException(
                "Невозможно получить JSON-значение: данному пути не соответствует ни один токен"
            );
        }

        if (checkedType is not null && !CheckJsonType(jResult, checkedType.Value))
        {
            throw new RuntimeException(
                $"Невозможно получить JSON-значение: полученный тип ({jResult.Type}) не соответствует ожидаемому ({checkedType})"
            );
        }

        return IntoOneScriptType(jResult);
    }

    private IValue GetSingleValueByIndex(int index, JsonDataTypeEnum? checkedType = null)
    {
        if (_root is not JArray rootArray)
        {
            throw new RuntimeException(
                "Невозможно получить JSON-значение по индексу: объект не является массивом"
            );
        }

        var jResult = rootArray[index];

        if (jResult is null)
        {
            throw new RuntimeException(
                "Невозможно получить JSON-значение: данному пути не соответствует ни один токен"
            );
        }

        if (checkedType is not null && !CheckJsonType(jResult, checkedType.Value))
        {
            throw new RuntimeException(
                $"Невозможно получить JSON-значение: полученный тип ({jResult.Type}) не соответствует ожидаемому ({checkedType})"
            );
        }
        
        return IntoOneScriptType(jResult);
    }

    private bool CheckJsonType(JToken value, JsonDataTypeEnum checkedType)
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

    private IValue IntoOneScriptType(JToken token, bool unwrapIfArray = false)
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

                JTokenType.Guid =>
                    ValueFactory.Create(token.Value<string>()),

                JTokenType.Uri =>
                    ValueFactory.Create(token.Value<string>()),

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