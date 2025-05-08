namespace APIGateway.Infrastructure.Interface.Ratelimiter
{
    public interface IRateLimit
    {
        bool IsAllowRequest(string userId, string route);
    }
}
