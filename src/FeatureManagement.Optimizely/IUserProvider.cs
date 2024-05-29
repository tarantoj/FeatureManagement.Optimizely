using OptimizelySDK.Entity;

namespace TarantoJ.FeatureManagement.Optimizely;

/// <summary>
/// [TODO:description]
/// </summary>
public interface IUserProvider
{
    /// <summary>
    /// [TODO:description]
    /// </summary>
    /// <returns>[TODO:return]</returns>
    Task<(string userId, UserAttributes? userAttributes)> GetUser();
}
