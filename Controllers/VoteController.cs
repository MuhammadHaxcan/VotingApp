using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VotingApp.Data;
using VotingApp.Models;

public class VoteController : Controller
{
    private readonly VotingDbContext _context;

    public VoteController(VotingDbContext context)
    {
        _context = context;
    }

    // List All Elections Available for Voting
    public async Task<IActionResult> Elections()
    {
        if (!IsUserLoggedIn()) return RedirectToAction("Login", "Auth");

        var elections = await _context.Elections
            .ToListAsync();

        return View(elections);
    }

    // View Candidates in Selected Election
    public async Task<IActionResult> Candidates(int electionId)
    {
        if (!IsUserLoggedIn()) return RedirectToAction("Login", "Auth");

        var election = await _context.Elections
            .Include(e => e.Candidates)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null) return NotFound();

        // Check if the user has already voted
        int userId = GetCurrentUserId();
        bool hasVoted = await _context.Votes.AnyAsync(v => v.UserId == userId && v.ElectionId == electionId);
        ViewBag.HasVoted = hasVoted;

        return View(election);
    }

    // Cast Vote
    [HttpPost]
    public async Task<IActionResult> CastVote(int candidateId, int electionId)
    {
        if (!IsUserLoggedIn()) return RedirectToAction("Login", "Auth");

        int userId = GetCurrentUserId();

        // Ensure the user has not voted before
        bool alreadyVoted = await _context.Votes.AnyAsync(v => v.UserId == userId && v.ElectionId == electionId);
        if (alreadyVoted)
        {
            TempData["Error"] = "You have already voted in this election.";
            return RedirectToAction("Elections");
        }

        // Validate Candidate Exists
        var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == candidateId && c.ElectionId == electionId);
        if (candidate == null) return NotFound();

        // Record the Vote
        var vote = new Vote
        {
            UserId = userId,
            CandidateId = candidateId,
            ElectionId = electionId
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Your vote has been recorded successfully!";
        return RedirectToAction("Elections");
    }

    // View Election Details (Shows Winner if Ended)
    public async Task<IActionResult> ElectionDetails(int electionId)
    {
        if (!IsUserLoggedIn()) return RedirectToAction("Login", "Auth");

        var election = await _context.Elections
            .Include(e => e.Candidates)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null) return NotFound();

        // Fetch vote counts per candidate
        var candidateVotes = await _context.Votes
            .Where(v => v.ElectionId == electionId)
            .GroupBy(v => v.CandidateId)
            .Select(g => new
            {
                CandidateId = g.Key,
                VoteCount = g.Count()
            })
            .ToListAsync();

        // Map votes to candidates
        foreach (var candidate in election.Candidates)
        {
            candidate.Votes = new List<Vote>(); // Ensure not null
            var voteCount = candidateVotes.FirstOrDefault(v => v.CandidateId == candidate.Id)?.VoteCount ?? 0;
            ViewBag.VoteCounts ??= new Dictionary<int, int>();
            ViewBag.VoteCounts[candidate.Id] = voteCount;
        }

        // Determine the winner if election has ended
        if (election.EndDate < System.DateTime.UtcNow)
        {
            var winner = election.Candidates
                .OrderByDescending(c => ViewBag.VoteCounts[c.Id])
                .FirstOrDefault();

            ViewBag.Winner = winner;
        }

        return View(election);
    }

    // Helper Methods
    private bool IsUserLoggedIn() => HttpContext.Session.GetString("UserId") != null;
    private int GetCurrentUserId() => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
}
