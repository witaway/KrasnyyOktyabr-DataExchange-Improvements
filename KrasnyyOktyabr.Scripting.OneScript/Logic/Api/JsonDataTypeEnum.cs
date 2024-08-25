using System;
using ScriptEngine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[EnumerationType("ДТип", "DType")]
public enum JsonDataTypeEnum
{
    [EnumItem("Булев", "Bool")]
    Boolean,
    
    [EnumItem("Строка", "String")]
    String,
    
    [EnumItem("Дата", "Date")]
    Date,
    
    [EnumItem("Число", "Number")]
    Number,
    
    [EnumItem("Объект", "Object")]
    Object,
    
    [EnumItem("Массив", "Array")]
    Array
}