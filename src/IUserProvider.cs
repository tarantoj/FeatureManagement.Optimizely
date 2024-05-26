using OptimizelySDK.Entity;

namespace TarantoJ.FeatureManagement.Optimizely;

public interface IUserProvider
{
    Task<(string userId, UserAttributes? userAttributes)> GetUser();
}
