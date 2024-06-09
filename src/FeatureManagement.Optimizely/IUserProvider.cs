using OptimizelySDK.Entity;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// A provider of user information
/// </summary>
public interface IUserProvider
{
    /// <summary>
    /// Gets the current user
    /// </summary>
    /// <returns>A unique user id with optional user attributes</returns>
    Task<(string userId, UserAttributes? userAttributes)> GetUser();
}
