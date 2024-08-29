using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[GlobalContext]
public class HelpersLegacy : GlobalContextBase<HelpersLegacy>
{
    public static IAttachableContext CreateInstance()
    {
        return (IAttachableContext)new HelpersLegacy();
    }

    [ContextMethod("СтрШаблонЛегаси", "StrTemplateLegacy")]
    public string StrTemplate(
        string template,
        IValue p1 = null, IValue p2 = null, IValue p3 = null, IValue p4 = null, IValue p5 = null, 
        IValue p6 = null, IValue p7 = null, IValue p8 = null, IValue p9 = null, IValue p10 = null)
    {
        var oscArgs = new List<IValue>() { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 };
        var clrArgs  = oscArgs
            .Select(ContextValuesMarshaller.ConvertToCLRObject)
            .Where(obj => obj is not null)
            .ToArray();
        return string.Format(template, clrArgs);
    }

    [ContextMethod("РегВыражБыстрыйПоиск", "RegexLegacy")]
    public string RegexLegacy(string regexString, string input, int groupNumber = 1)
    {
        Regex regex = new(regexString);
        Match match = regex.Match(input);

        return match.Success
            ? match.Groups[groupNumber].Value
            : string.Empty;   
    }
}