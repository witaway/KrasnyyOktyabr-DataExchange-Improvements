using Newtonsoft.Json.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

public partial class JsonData
{
    public override IValue GetIndexedValue(IValue index)
    {
        switch (index.DataType)
        {
            case DataType.String:
            {
                return GetSingleValueByPath(
                    index.AsString(),
                    CheckedTypeForIndexOperatorAccess
                );
            }
            case DataType.Number:
            {
                return GetSingleValueByIndex(
                    (int)index.AsNumber(),
                    CheckedTypeForIndexOperatorAccess
                );
            }
            case DataType.Enumeration:
            {
                return new JsonData(
                    Root,
                    false,
                    ContextValuesMarshaller.CastToCLRObject<JsonDataTypeEnum>(index)
                );
            }
            default:
            {
                return base.GetIndexedValue(index);
            }
        }
    }

    [ContextMethod("Получить", "Get")]
    public IValue GetSingleValue(IValue pathOrIndex, JsonDataTypeEnum? checkedType = null)
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

    protected IValue GetSingleValueByPath(string path, JsonDataTypeEnum? checkedType = null)
    {
        var jResult = Root.SelectToken(path);

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

    protected IValue GetSingleValueByIndex(int index, JsonDataTypeEnum? checkedType = null)
    {
        if (Root is not JArray rootArray)
        {
            throw new RuntimeException(
                "Невозможно получить JSON-значение по индексу: сущность не является массивом"
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
}