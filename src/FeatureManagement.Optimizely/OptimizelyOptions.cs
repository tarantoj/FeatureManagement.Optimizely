using System.ComponentModel.DataAnnotations;

namespace TarantoJ.FeatureManagement.Optimizely;

public class OptimizelyOptions
{
    [Required]
    public string SdkKey { get; init; }
    public bool? Logging { get; set; }
}
