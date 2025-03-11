namespace VotingApp.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CandidateId { get; set; }
        public int ElectionId { get; set; }
        public DateTime VoteTime { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public Candidate Candidate { get; set; }
        public Election Election { get; set; }
    }
}
