using Newtonsoft.Json;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Kafka;

public class V83ApplicationProducerNewTransactionRequest
{
    [JsonProperty("transactionTypeFilters")]
    public string[] TransactionTypeFilters { get; set; }

    [JsonProperty("objectFilters")]
    public V83ApplicationProducerNewTransactionRequestObjectFilter[] ObjectFilters { get; set; }

    [JsonProperty("transactionToStartAfter")]
    public string TransactionToStartAfter { get; set; }

    [JsonProperty("startDate")]
    public string StartDate { get; set; }
}
