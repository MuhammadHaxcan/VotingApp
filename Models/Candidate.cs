namespace VotingApp.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public int UserId { get; set; } // Foreign Key to User
        public string Password { get; set; } // ✅ Store the raw password

        public Election Election { get; set; }
        public User User { get; set; }
        public ICollection<Vote> Votes { get; set; }
    }
}