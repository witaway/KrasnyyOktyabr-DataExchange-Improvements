using KrasnyyOktyabr.Scripting.OneScript.Logic.Helpers;
using Newtonsoft.Json.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

public partial class JsonData
{
    public override void SetIndexedValue(IValue index, IValue value)
    {
        switch (index.DataType)
        {
            case DataType.String:
            {
                SetSingleValueByPath(
                    index.AsString(),
                    value,
                    _checkedTypeForIndexOperatorAccess
                );
                break;
            }
            case DataType.Number:
            {
                SetSingleValueByIndex(
                    (int)index.AsNumber(),
                    value,
                    _checkedTypeForIndexOperatorAccess
                );
                break;
            }
            default:
            {
                base.SetIndexedValue(index, value);
                break;
            }
        }
    }

    [ContextMethod("Установить", "Set")]
    public IValue SetSingleValue(IValue pathOrIndex, IValue value, JsonDataTypeEnum? checkedType = null)
    {
        switch (pathOrIndex.DataType)
        {
            case DataType.String:
            {
                var path = pathOrIndex.AsString();
                SetSingleValueByPath(path, value, checkedType);
                return value;
            }
            case DataType.Number:
            {
                var index = (int)pathOrIndex.AsNumber();
                SetSingleValueByIndex(index, value, checkedType);
                return value;
            }
            default:
                throw new RuntimeException("Данные.Установить ожидает получить строку или число как первый аргумент");
        }
    }

    protected void SetSingleValueByPath(string path, IValue value, JsonDataTypeEnum? checkedType = null)
    {
        if (_root is not JObject rootObject)
        {
            throw new RuntimeException(
                "Невозможно установить JSON-значение по индексу: сущность не является массивом"
            );
        }

        var jValue = IntoJsonType(value);

        if (checkedType is not null && !CheckJsonType(jValue, checkedType.Value))
        {
            throw new RuntimeException(
                $"Невозможно установить JSON-значение: полученный тип ({jValue.Type}) не соответствует ожидаемому ({checkedType})"
            );
        }

        rootObject.Add(path, jValue);
    }

    protected void SetSingleValueByIndex(int index, IValue value, JsonDataTypeEnum? checkedType = null)
    {
        if (_root is not JArray rootArray)
        {
            throw new RuntimeException(
                "Невозможно установить JSON-значение по индексу: сущность не является массивом"
            );
        }

        var jValue = IntoJsonType(value);

        if (checkedType is not null && !CheckJsonType(jValue, checkedType.Value))
        {
            throw new RuntimeException(
                $"Невозможно установить JSON-значение: полученный тип ({jValue.Type}) не соответствует ожидаемому ({checkedType})"
            );
        }

        rootArray.ExtendLength(index);
        rootArray[index] = jValue;
    }
}