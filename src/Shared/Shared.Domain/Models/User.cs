using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.Domain.Models
{
    public class User
    {
        public Guid UserId { get; init; } = Guid.NewGuid();
        public string? UserName { get; set; }
        private string _userEmail; // once finish , need to convert to Value Object
        public string? UserEmail { 
            get => _userEmail;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Invalid email format.", nameof(UserEmail));

                _userEmail = value;
            } 
        }
        public string? UserPhone { get; set; }

        [MaxLength(255)]
        public string FirstName { get; set; }
        [MaxLength(255)]
        public string LastName { get; set; }

        [MaxLength(255)]
        public string NickName { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public virtual ICollection<Idea> Ideas { get; set; }
        public virtual ICollection<ExpiredIdea> ExpiredIdeas { get; set; }
        public virtual ICollection<Collection> AccessCollections { get; set; }
        public virtual ICollection<Collection> OwnedCollections { get; set; }
        public virtual ICollection<TagIdea> TagIdeas { get; set; }
        public virtual ICollection<TagExpiredIdea> TagExpiredIdeas { get; set; }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
