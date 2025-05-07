using FluentValidation;
using System.ComponentModel.DataAnnotations;
namespace APIGateway.API.Model.DTO
{
    public class UserContext
    {
        [Required]
        public Guid UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        public DateTime AuthenticatedAt { get; set; }
        public List<string> Roles { get; set; }
    }
}
