using Newtonsoft.Json;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Kafka;
public class V83ApplicationProducerNewTransactionRequestObjectFilter
{
    [JsonProperty("dataType")]
    public string DataType { get; set; }

    [JsonProperty("depth")]
    public int JsonDepth { get; set; }
}
