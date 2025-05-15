namespace APIGateway.Infrastructure.Interface.Ratelimiter
{
    public interface IRateLimit
    {
        Task<bool> IsAllowRequestAsync(string userId, Guid moduleId);
    }
}
