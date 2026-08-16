using System.ComponentModel.DataAnnotations;
using Xunit;

namespace TarantoJ.FeatureManagement.Optimizely.Tests;

public class OptimizelyOptionsTests
{
    [Fact]
    public void SdkKey_IsRequired()
    {
        var options = new OptimizelyOptions { SdkKey = string.Empty };

        var results = Validate(options);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(OptimizelyOptions.SdkKey)));
    }

    [Fact]
    public void Logging_IsEnabledByDefault() =>
        Assert.True(new OptimizelyOptions().Logging);

    [Fact]
    public void DefaultUserId_IsAnonymousUserByDefault() =>
        Assert.Equal("anonymous-user", new OptimizelyOptions().DefaultUserId);

    [Fact]
    public void SectionName_IsOptimizely() =>
        Assert.Equal("Optimizely", OptimizelyOptions.SectionName);

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true
        );
        return results;
    }
}
