using System.ComponentModel.DataAnnotations;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Configuration.Kafka;

public class AbstractVApplicationProducerSettings : AbstractSuspendableSettings
{
    public string Username { get; set; }

    public string Password { get; set; }

    [Required]
    public string DataTypePropertyName { get; set; }

    [Required]
    public string[] TransactionTypeFilters { get; set; }
}
