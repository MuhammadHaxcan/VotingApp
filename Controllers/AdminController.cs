using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;
using System.Security.Cryptography;
using System.Text;


public class AdminController : Controller
{
    private readonly VotingDbContext _context;

    public AdminController(VotingDbContext context)
    {
        _context = context;
    }

    // Dashboard View (Only for Admin)
    public IActionResult Dashboard()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // Auto-update election statuses
        UpdateElectionStatuses();

        return View();
    }

    // Create Election (GET)
    public IActionResult CreateElection()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        return View();
    }

    // Create Election (POST)
    [HttpPost]
    public async Task<IActionResult> CreateElection(Election model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        ModelState.Remove("Votes");
        ModelState.Remove("Status");
        ModelState.Remove("Candidates");

        // Validation
        if (model.StartDate < DateTime.Now.Date)
        {
            ModelState.AddModelError("StartDate", "Start date must be today or in the future.");
        }
        if (model.EndDate <= model.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date must be later than the start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Set election status automatically
            model.Status = GetElectionStatus(model.StartDate, model.EndDate);

            _context.Elections.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Election created successfully!";
            return RedirectToAction("Elections");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while creating the election.";
            return View(model);
        }
    }

    // View All Elections
    public async Task<IActionResult> Elections()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        UpdateElectionStatuses(); // Ensure statuses are updated in DB
        var elections = await _context.Elections.AsNoTracking().ToListAsync(); // Fetch fresh data

        return View(elections);
    }

    // Edit Election (GET)
    public async Task<IActionResult> EditElection(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var election = await _context.Elections.FindAsync(id);
        if (election == null)
        {
            TempData["ErrorMessage"] = "Election not found.";
            return RedirectToAction("Elections");
        }

        return View(election);
    }

    // Edit Election (POST)
    [HttpPost]
    public async Task<IActionResult> EditElection(Election model)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        ModelState.Remove("Votes");
        ModelState.Remove("Status");
        ModelState.Remove("Candidates");

        // Validation
        if (model.StartDate < DateTime.Now.Date)
        {
            ModelState.AddModelError("StartDate", "Start date must be today or in the future.");
        }
        if (model.EndDate <= model.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date must be later than the start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var election = await _context.Elections.FindAsync(model.Id);
            if (election == null)
            {
                TempData["ErrorMessage"] = "Election not found.";
                return RedirectToAction("Elections");
            }

            // Update election details
            election.Name = model.Name;
            election.StartDate = model.StartDate;
            election.EndDate = model.EndDate;
            election.Status = GetElectionStatus(model.StartDate, model.EndDate); // Auto-update status

            _context.Elections.Update(election);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Election updated successfully!";
            return RedirectToAction("Elections");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while updating the election.";
            return View(model);
        }
    }

    // Delete Election
    public async Task<IActionResult> DeleteElection(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var election = await _context.Elections.FindAsync(id);
        if (election == null)
        {
            TempData["ErrorMessage"] = "Election not found.";
            return RedirectToAction("Elections");
        }

        try
        {
            _context.Elections.Remove(election);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Election deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the election.";
        }

        return RedirectToAction("Elections");
    }

    // Create Candidate (GET)
    public IActionResult CreateCandidate()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        ViewBag.Elections = _context.Elections.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCandidate(string name, string email, string nic, int electionId)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        // Manually validate NIC length
        if (nic.Length != 13)
        {
            ModelState.AddModelError("nic", "NIC must be exactly 13 characters long.");
        }

        // Check if email or NIC exists
        if (await _context.Users.AnyAsync(u => u.Email == email || u.NIC == nic))
        {
            ModelState.AddModelError("", "Email or NIC already exists.");
        }

        // Check if the model state is valid
        if (!ModelState.IsValid)
        {
            ViewBag.Elections = _context.Elections.ToList();
            return View();
        }

        try
        {
            // Generate Random Password
            var password = GenerateRandomPassword();

            // Create Candidate Account
            var user = new User
            {
                Name = name,
                Email = email,
                NIC = nic,
                PasswordHash = HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign Role (Candidate)
            var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
            if (candidateRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = candidateRole.Id
                });
                await _context.SaveChangesAsync();
            }

            // Assign Candidate to Election & Store Password
            var candidate = new Candidate
            {
                UserId = user.Id,
                ElectionId = electionId,
                Password = password // Store the raw password in Candidate table
            };

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Candidate created successfully!";
            return RedirectToAction("Users");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while creating the candidate.";
            ViewBag.Elections = _context.Elections.ToList();
            return View();
        }
    }
    // Edit Candidate (GET)
    public async Task<IActionResult> EditCandidate(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var candidate = await _context.Candidates
            .Include(c => c.User)
            .Include(c => c.Election)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (candidate == null)
        {
            TempData["ErrorMessage"] = "Candidate not found.";
            return RedirectToAction("Users");
        }

        ViewBag.Elections = await _context.Elections.ToListAsync(); // Provide elections list for selection
        return View(candidate);
    }

    // Edit Candidate (POST)
    [HttpPost]
    public async Task<IActionResult> EditCandidate(int id, string name, string email, string nic, int electionId, string password)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var candidate = await _context.Candidates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
        if (candidate == null)
        {
            TempData["ErrorMessage"] = "Candidate not found.";
            return RedirectToAction("Users");
        }

        try
        {
            // Update User Information
            candidate.User.Name = name;
            candidate.User.Email = email;
            candidate.User.NIC = nic;
            candidate.ElectionId = electionId;

            // Update Password if changed
            if (!string.IsNullOrEmpty(password))
            {
                candidate.Password = password;
                candidate.User.PasswordHash = HashPassword(password);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Candidate updated successfully!";
            return RedirectToAction("Users");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while updating the candidate.";
            ViewBag.Elections = await _context.Elections.ToListAsync();
            return View(candidate);
        }
    }

    // Delete Candidate
    public async Task<IActionResult> DeleteCandidate(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var candidate = await _context.Candidates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
        if (candidate == null)
        {
            TempData["ErrorMessage"] = "Candidate not found.";
            return RedirectToAction("Users");
        }

        try
        {
            // Remove Candidate from DB
            _context.Candidates.Remove(candidate);

            // Remove User associated with Candidate
            _context.Users.Remove(candidate.User);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Candidate deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the candidate.";
        }

        return RedirectToAction("Users");
    }

    // View Election Details (Admin)
    public async Task<IActionResult> ElectionDetails(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var election = await _context.Elections
            .Include(e => e.Candidates)
            .ThenInclude(c => c.User) // Load Candidate Names
            .Include(e => e.Candidates)
            .ThenInclude(c => c.Votes) // Ensure Votes are included
            .FirstOrDefaultAsync(e => e.Id == id);

        if (election == null)
        {
            TempData["ErrorMessage"] = "Election not found.";
            return RedirectToAction("Elections");
        }

        // Determine the winner (if election is completed)
        Candidate winner = null;
        if (election.Status == "Completed" && election.Candidates.Any())
        {
            winner = election.Candidates
                .OrderByDescending(c => c.Votes.Count)
                .FirstOrDefault();
        }

        ViewBag.Winner = winner;
        return View(election);
    }

    // View All Users
    public async Task<IActionResult> Users()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Auth");

        var users = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Candidates)
            .ThenInclude(c => c.Election)
            .ToListAsync();

        return View(users);
    }

    // Check if User is Admin
    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    // Generate Random Password for Candidates
    private string GenerateRandomPassword()
    {
        return "Candidate@" + new Random().Next(1000, 9999);
    }

    // Password Hashing Method
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    // Method to update election statuses
    private void UpdateElectionStatuses()
    {
        var elections = _context.Elections.ToList();
        foreach (var election in elections)
        {
            string newStatus = GetElectionStatus(election.StartDate, election.EndDate);
            if (election.Status != newStatus)
            {
                election.Status = newStatus;
            }
        }
        _context.SaveChanges(); // Ensure updated statuses are stored in DB
    }

    // Determines the correct status for an election based on the date
    private string GetElectionStatus(DateTime startDate, DateTime endDate)
    {
        DateTime now = DateTime.Now;

        if (now < startDate)
            return "Upcoming";
        else if (now >= startDate && now <= endDate)
            return "Ongoing";
        else
            return "Completed";
    }
}