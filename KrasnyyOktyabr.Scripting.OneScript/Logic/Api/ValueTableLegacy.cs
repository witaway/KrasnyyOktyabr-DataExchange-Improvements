using System.Collections.Generic;
using System.Linq;
using KrasnyyOktyabr.JsonTransform.Structures;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[ContextClass("ТаблицаЗначенийЛегаси", "ValueTableLegacy")]
public class ValueTableLegacy
{
    private ValueTable _valueTable;

    public ValueTableLegacy(string columnsString)
    {
        IEnumerable<string> columns = columnsString
            .Split(',')
            .Select(c => c.Trim());

        _valueTable = new(columns);
    }

    [ScriptConstructor]
    public static ValueTableLegacy Constructor(string columnsString)
    {
        return new ValueTableLegacy(columnsString);
    }

    [ContextMethod("ДобавитьСтолбец", "AddColumn")]
    public void AddColumn(string columnName)
    {
        _valueTable.AddColumn(columnName);
    }

    [ContextMethod("ДобавитьСтроку", "AddLine")]
    public void AddLine()
    {
        _valueTable.AddLine();
    }

    [ContextMethod("Сжать", "Collapse")]
    public void Collapse(string columnsToGroupString, string? columnsToSumString = null)
    {
        IEnumerable<string> columnsToGroup = columnsToGroupString
            .Split(',')
            .Select(c => c.Trim());

        if (columnsToSumString is not null)
        {
            IEnumerable<string> columnsToSum = columnsToSumString
                .Split(',')
                .Select(c => c.Trim());

            _valueTable.Collapse(columnsToGroup, columnsToSum);
        }
        else
        {
            _valueTable.Collapse(columnsToGroup);
        }
    }

    [ContextMethod("Количество", "Count")]
    public int Count()
    {
        return _valueTable.Count;
    }

    [ContextMethod("ПолучитьЗначение", "GetValue")]
    public IValue GetValue(string columnName)
    {
        var value = _valueTable.GetValue(columnName);
        return ContextValuesMarshaller.ConvertDynamicValue(value);
    }

    [ContextMethod("УстановитьЗначение", "SetValue")]
    public void SetValue(string columnName, IValue value)
    {
        var valueClr = ContextValuesMarshaller.ConvertToCLRObject(value);
        _valueTable.SetValue(columnName, valueClr);
    }

    [ContextMethod("ВыбратьСтроку", "SelectLine")]
    public void SelectLine(int line)
    {
        _valueTable.SelectLine(line);
    }
}