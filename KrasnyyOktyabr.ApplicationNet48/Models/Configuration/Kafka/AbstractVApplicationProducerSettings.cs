using System.ComponentModel.DataAnnotations;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Configuration.Kafka;

public class AbstractVApplicationProducerSettings : AbstractSuspendableSettings
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string DataTypePropertyName { get; set; }

    [Required]
    public VApplicationObjectFilter[] ObjectFilters { get; set; }

    [Required]
    public string[] TransactionTypeFilters { get; set; }
}
