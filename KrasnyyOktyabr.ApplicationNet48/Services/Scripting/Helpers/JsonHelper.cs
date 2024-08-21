using System.Collections.Generic;

namespace KrasnyyOktyabr.ApplicationNet48.Services.Helpers;

public static class JsonHelper
{
    public readonly struct JsonTransformMsSqlResult(string table, Dictionary<string, dynamic> columnValues)
    {
        public string Table { get; } = table;

        public Dictionary<string, dynamic> ColumnValues { get; } = columnValues;
    }

    public struct KafkaProducerMessageData(string objectJson, string? dataType)
    {
        public readonly string ObjectJson { get; } = objectJson;

        public readonly string? DataType { get; } = dataType;

        public string? TopicName { get; set; }
    }
}