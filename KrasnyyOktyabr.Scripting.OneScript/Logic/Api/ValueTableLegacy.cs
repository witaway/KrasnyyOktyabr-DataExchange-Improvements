using System.Collections.Generic;
using System.Linq;
using KrasnyyOktyabr.JsonTransform.Numerics;
using KrasnyyOktyabr.JsonTransform.Structures;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[ContextClass("ТаблицаЗначенийЛегаси", "ValueTableLegacy")]
public class ValueTableLegacy : AutoContext<ValueTableLegacy>
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

    [ContextMethod("КоличествоСтрок", "RowsCount")]
    public int Count()
    {
        return _valueTable.Count;
    }

    [ContextMethod("ПолучитьЗначение", "GetValue")]
    public IValue GetValue(string columnName)
    {
        return _valueTable.GetValue(columnName) switch
        {
            Number numberValue => ValueFactory.Create((numberValue.Long ?? numberValue.Decimal)!.Value),
            string stringValue => ValueFactory.Create(stringValue),
            IValue objectValue => objectValue,
            _ => throw new RuntimeException("ValueTableLegacy не может получить значение поскольку хранимый тип не правилен.")
        };
    }

    [ContextMethod("УстановитьЗначение", "SetValue")]
    public void SetValue(string columnName, IValue value)
    {
        _valueTable.SetValue(columnName, value.DataType switch
        {
            DataType.Number => new Number(value.AsNumber()),
            DataType.String => value.AsString(),
            _ => value
        });
    }

    [ContextMethod("ВыбратьСтроку", "SelectLine")]
    public void SelectLine(int line)
    {
        _valueTable.SelectLine(line);
    }
}