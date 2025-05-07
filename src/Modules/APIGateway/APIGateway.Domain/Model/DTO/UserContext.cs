using FluentValidation;

namespace APIGateway.Domain.Model.DTO
{
    public class UserContext
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
