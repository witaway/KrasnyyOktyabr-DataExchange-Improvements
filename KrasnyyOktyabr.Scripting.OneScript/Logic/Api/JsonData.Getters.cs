using System;
using Newtonsoft.Json.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

public partial class JsonData
{
    public override IValue GetIndexedValue(IValue index)
    {
        // If index is string or number,
        // perform search of corresponding children here
        // But if children was a json array or object,
        // In this case inherit (ONLY!) get and set flags here from parent
        if (index.DataType is DataType.String or DataType.Number)
        {
            var result = index.DataType switch
            {
                DataType.String => GetSingleValueByPath(
                    index.AsString(),
                    CheckedTypeForIndexOperatorAccess
                ),
                DataType.Number => GetSingleValueByIndex(
                    (int)index.AsNumber(),
                    CheckedTypeForIndexOperatorAccess
                )
            };
            if (result is JsonData jsonDataResult)
            {
                jsonDataResult.GetNothingFoundBehaviour = GetNothingFoundBehaviour;
                jsonDataResult.SetAlreadyExistsBehaviour = SetAlreadyExistsBehaviour;
            }

            return result;
        }

        // In case index is one of flags,
        // just return the copy of JsonData, but change the corresponding flag in it.
        if (index.DataType is DataType.Enumeration)
        {
            var newJsonData = new JsonData(this);
            object unknownEnum = ContextValuesMarshaller.CastToCLRObject(index);

            if (unknownEnum is JsonDataTypeEnum jsonDataTypeEnum)
            {
                newJsonData.CheckedTypeForIndexOperatorAccess = jsonDataTypeEnum;
                return newJsonData;
            }

            if (unknownEnum is JsonDataGetFlagsEnum jsonDataGetFlagsEnum)
            {
                newJsonData.GetNothingFoundBehaviour = jsonDataGetFlagsEnum;
                return newJsonData;
            }

            if (unknownEnum is JsonDataSetFlagsEnum jsonDataSetFlagsEnum)
            {
                newJsonData.SetAlreadyExistsBehaviour = jsonDataSetFlagsEnum;
                return newJsonData;
            }
        }

        return base.GetIndexedValue(index);
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
            return GetNothingFoundBehaviour switch
            {
                JsonDataGetFlagsEnum.ReturnNull => ValueFactory.CreateNullValue(),
                JsonDataGetFlagsEnum.Error => throw new RuntimeException(
                    "Невозможно получить JSON-значение: данному пути не соответствует ни один токен"
                ),
                _ => throw new ArgumentOutOfRangeException()
            };
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

        if (index >= rootArray.Count)
        {
            return GetNothingFoundBehaviour switch
            {
                JsonDataGetFlagsEnum.ReturnNull => ValueFactory.CreateNullValue(),
                JsonDataGetFlagsEnum.Error => throw new RuntimeException(
                    "Невозможно получить JSON-значение: индекс превышает размер массива"
                ),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        var jResult = rootArray[index];

        if (checkedType is not null && !CheckJsonType(jResult, checkedType.Value))
        {
            throw new RuntimeException(
                $"Невозможно получить JSON-значение: полученный тип ({jResult.Type}) не соответствует ожидаемому ({checkedType})"
            );
        }

        return IntoOneScriptType(jResult);
    }
}