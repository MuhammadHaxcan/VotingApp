using System.ComponentModel.DataAnnotations;
namespace VotingApp.Models
{
    public class Candidate
    {
        public int Id { get; set; }

        [Required]
        public int ElectionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        // Navigation properties
        public Election Election { get; set; }
        public User User { get; set; }
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}