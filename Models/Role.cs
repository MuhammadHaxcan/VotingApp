namespace VotingApp.Models
{
    public class Role
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } // Role Name (Admin, Voter, Candidate)
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
