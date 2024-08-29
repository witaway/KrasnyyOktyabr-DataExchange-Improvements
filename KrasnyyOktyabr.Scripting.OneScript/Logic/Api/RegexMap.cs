using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

    [ContextClass("РегулярноеСоответствие", "RegexMap")]
public class RegexMap : AutoContext<RegexMap>, ICollectionContext, IEnumerable, IEnumerable<KeyAndValueImpl>
{
    private Dictionary<string, string> _dictionary = new();

    public RegexMap()
    {
    }

    public RegexMap(string inputString, string? regex = null, string? keyGroup = null, string? valueGroup = null)
    {
        regex ??= @"(?<key>[^;]+)=(?<value>[^;]+)";
        keyGroup ??= "key";
        valueGroup ??= "value";

        _dictionary = Regex
            .Matches(inputString, regex)
            .Cast<Match>()
            .ToDictionary(
                m => m.Groups[keyGroup].Value,
                m => m.Groups[valueGroup].Value
            );
    }

    [ScriptConstructor]
    public static RegexMap Constructor(IValue inputString, IValue regex, IValue keyGroup, IValue valueGroup)
    {
        return new RegexMap(
            inputString.AsString(),
            regex.AsString(),
            keyGroup.AsString(),
            valueGroup.AsString()
        );
    }

    [ContextMethod("Количество", "Count")]
    public int Count()
    {
        return _dictionary.Count;
    }

    public override bool IsIndexed => true;

    public override IValue GetIndexedValue(IValue index)
    {
        if (!_dictionary.TryGetValue(index.AsString(), out string indexedValue))
            return ValueFactory.Create();
        return ValueFactory.Create(indexedValue);
    }

    [ContextMethod("Получить", "Get")]
    public IValue Retrieve(IValue key) => GetIndexedValue(key);

    public CollectionEnumerator GetManagedIterator()
    {
        return new CollectionEnumerator((IEnumerator<IValue>)this.GetEnumerator());
    }

    public IEnumerator<KeyAndValueImpl> GetEnumerator()
    {
        foreach (KeyValuePair<string, string> keyValuePair in this._dictionary)
            yield return new KeyAndValueImpl(
                ValueFactory.Create(keyValuePair.Key),
                ValueFactory.Create(keyValuePair.Value)
            );
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
}