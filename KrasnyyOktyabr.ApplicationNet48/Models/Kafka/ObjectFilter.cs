#nullable enable

using Newtonsoft.Json;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Kafka;

public readonly struct ObjectFilter(string id, int depth, string? topic)
{
    [JsonProperty("id")]
    public string Id { get; } = id;

    [JsonProperty("depth")]
    public int Depth { get; } = depth;

#nullable enable
    [JsonProperty("topic")]
    public string? Topic { get; } = topic;
}
