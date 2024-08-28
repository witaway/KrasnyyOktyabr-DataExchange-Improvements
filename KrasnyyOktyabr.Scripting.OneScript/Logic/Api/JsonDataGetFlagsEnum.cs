using ScriptEngine;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[EnumerationType("ПНеНайден", "BNotFound")]
public enum JsonDataGetFlagsEnum
{
    [EnumItem("Нуль", "Null")]
    ReturnNull = 0,
    
    [EnumItem("Ошибка", "Error")]
    Error = 1,
}