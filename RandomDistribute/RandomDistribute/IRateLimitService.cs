namespace RandomDistribute
{
    public interface IRateLimitService
    {
        bool IsCustomUrlLimitExceeded(string userId);
        bool IsLimitExceeded(string userId, int limitMultiplier = 1);
    }
}