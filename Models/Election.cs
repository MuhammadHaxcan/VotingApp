using System.ComponentModel.DataAnnotations;

namespace VotingApp.Models
{
    public class Election
    {
        public int Id { get; set; }

        [Required, StringLength(100, ErrorMessage = "Election name must be between 3 and 100 characters.", MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string Status { get; set; }
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}