using System.ComponentModel.DataAnnotations;

namespace KrasnyyOktyabr.ApplicationNet48.Models.Configuration.Kafka;

public class VApplicationObjectFilter
{
    [Required]
    public string IdPrefix { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Only natural numbers allowed")]
    public int JsonDepth { get; set; }

#nullable enable
    public string? Topic { get; set; }
}
