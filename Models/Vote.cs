using System.ComponentModel.DataAnnotations;
namespace VotingApp.Models
{
    public class Vote
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int ElectionId { get; set; }

        public DateTime VoteTime { get; set; } = DateTime.Now;

        // Navigation properties
        public User User { get; set; }
        public Candidate Candidate { get; set; }
        public Election Election { get; set; }
    }
}