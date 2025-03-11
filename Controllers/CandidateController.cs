using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VotingApp.Data;
using VotingApp.Models;

public class CandidateController : Controller
{
    private readonly VotingDbContext _context;

    public CandidateController(VotingDbContext context)
    {
        _context = context;
    }

    // Candidate Dashboard: View Elections the Candidate is Part Of
    public async Task<IActionResult> Dashboard()
    {
        if (!IsCandidate()) return RedirectToAction("Login", "Auth");

        int userId = GetCurrentUserId();

        // Get elections where the logged-in user is a candidate
        var elections = await _context.Candidates
            .Where(c => c.UserId == userId)
            .Include(c => c.Election)
            .ToListAsync();

        return View(elections);
    }

    // View Vote Details for a Specific Election
    public async Task<IActionResult> ElectionDetails(int electionId)
    {
        if (!IsCandidate()) return RedirectToAction("Login", "Auth");

        int userId = GetCurrentUserId();

        // Get the candidate's details for the selected election
        var candidate = await _context.Candidates
            .Include(c => c.Election)
            .Include(c => c.Votes)
            .ThenInclude(v => v.User) // Load users who voted
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ElectionId == electionId);

        if (candidate == null) return NotFound();

        return View(candidate);
    }

    // Helper Methods
    private bool IsCandidate()
    {
        return HttpContext.Session.GetString("UserRole") == "Candidate";
    }

    private int GetCurrentUserId()
    {
        return int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
    }
}
