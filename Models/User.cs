using System.ComponentModel.DataAnnotations;
namespace VotingApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(13, MinimumLength = 13, ErrorMessage = "NIC must be 13 digits.")]
        public string NIC { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}