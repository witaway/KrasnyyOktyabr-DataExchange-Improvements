using ScriptEngine;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[EnumerationType("ПУжеЕсть", "BAlreadySet")]
public enum JsonDataSetFlagsEnum
{
    [EnumItem("Пропуск", "Skip")]
    Skip = 0,

    [EnumItem("Замена", "Replace")]
    Replace = 1,
    
    [EnumItem("Ошибка", "Error")]
    Error = 2,
}