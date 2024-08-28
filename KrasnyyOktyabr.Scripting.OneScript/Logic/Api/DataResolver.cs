using System.Collections.Generic;
using KrasnyyOktyabr.DataResolve;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[GlobalContext(ManualRegistration = true)]
public class DataResolversFactoryContext(IDataResolveService dataResolveService)
    : GlobalContextBase<DataResolversFactoryContext>
{
    private IDataResolveService _dataResolveService = dataResolveService;

    [ContextMethod("ПолучательДанных", "DataResolver")]
    public DataResolver DataResolver(string resolverName)
    {
        return new DataResolver(_dataResolveService, resolverName);
    }
}

[ContextClass("ПолучательДанных", "DataResolver")]
public class DataResolver : AutoContext<DataResolver>
{
    private Dictionary<string, object> _arguments = new();
    private IDataResolveService _dataResolveService;
    private string _resolverName;
    
    public DataResolver(IDataResolveService dataResolveService, string resolverName)
    {
        _dataResolveService = dataResolveService;
        _resolverName = resolverName;
    }
    
    public override void SetIndexedValue(IValue index, IValue value)
    {
        var indexStr = index.AsString();
        
        if (indexStr is null)
        {
            throw new RuntimeException("Ключом ПолучателяДанных должны быть только строки!");
        }
        
        var valueClr = ContextValuesMarshaller.CastToCLRObject(value);
        
        _arguments.Add(indexStr, valueClr);
    }

    public override IValue GetIndexedValue(IValue index)
    {
        var indexStr = index.AsString();
        
        if (indexStr is null)
        {
            throw new RuntimeException("Ключом ПолучателяДанных должны быть только строки!");
        }
        
        var valueOscSuccess = _arguments.TryGetValue(indexStr, out var valueOsc);

        return valueOscSuccess 
            ? ContextValuesMarshaller.ConvertDynamicIndex(valueOsc) 
            : ValueFactory.CreateNullValue();
    }
    
    [ContextMethod("Выполнить", "Execute")]
    public IValue Execute()
    {
        
        var resolveTask = _dataResolveService.ResolveAsync(_resolverName, _arguments, new());
        var resolveResultClr = resolveTask.Result;
        var resolveResultOsc = ContextValuesMarshaller.ConvertDynamicValue(resolveResultClr);
        return resolveResultOsc;
    }
}