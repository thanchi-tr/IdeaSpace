using Crud.Application.Util;
using System.ComponentModel.DataAnnotations;

namespace Crud.Application.Data.DTO
{
    public class UserRestfulDTO
    {
        public Guid UserId { get; init; } = Guid.NewGuid();
        public string? UserName { get; set; }
        private string _userEmail; // once finish , need to convert to Value Object
        public string? UserEmail
        {
            get => _userEmail;
            set
            {
                if (!value.IsValidEmail())
                    throw new ArgumentException("Invalid email format.", nameof(UserEmail));

                _userEmail = value;
            }
        }
        public string? UserPhone { get; set; }

        [MaxLength(255)]
        public string? FirstName { get; set; }
        [MaxLength(255)]
        public string? LastName { get; set; }

        [MaxLength(255)]
        public string? NickName { get; set; } = string.Empty;
    }
}
