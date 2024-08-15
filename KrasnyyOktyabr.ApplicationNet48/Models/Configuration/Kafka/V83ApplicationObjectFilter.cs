using System.ComponentModel.DataAnnotations;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Configuration.Kafka;

public class V83ApplicationObjectFilter : AbstractVApplicationObjectFilter
{
    [Required]
    public string DataType { get; set; }
}
