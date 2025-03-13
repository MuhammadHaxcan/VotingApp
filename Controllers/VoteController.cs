using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

    // View All Elections
    public async Task<IActionResult> Elections()
    {
        // Update election statuses before displaying the list
        UpdateElectionStatuses();

        var elections = await _context.Elections
            .Include(e => e.Candidates)
            .ThenInclude(c => c.User)
            .ToListAsync();

        return View(elections);
    }

    // View Election Details
    public async Task<IActionResult> ElectionDetails(int electionId)
    {
        var election = await _context.Elections
            .Include(e => e.Candidates)
            .ThenInclude(c => c.Votes)
            .Include(e => e.Candidates)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null)
        {
            TempData["Error"] = "Election not found.";
            return RedirectToAction("Elections");
        }

        UpdateElectionStatuses();

        // Determine the winner if the election is completed
        Candidate winner = null;
        if (election.Status == "Completed")
        {
            winner = election.Candidates
                .OrderByDescending(c => c.Votes.Count)
                .FirstOrDefault();
        }

        // Calculate vote counts for each candidate
        var voteCounts = election.Candidates
            .ToDictionary(c => c.Id, c => c.Votes.Count);

        ViewBag.VoteCounts = voteCounts;
        ViewBag.Winner = winner;

        return View(election);
    }

    // View Candidates for an Election
    public async Task<IActionResult> Candidates(int electionId)
    {
        var election = await _context.Elections
            .Include(e => e.Candidates)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null)
        {
            TempData["Error"] = "Election not found.";
            return RedirectToAction("Elections");
        }

        // Check if the current user has already voted in this election
        int userId = GetCurrentUserId();
        bool hasVoted = await _context.Votes
            .AnyAsync(v => v.UserId == userId && v.ElectionId == electionId);

        ViewBag.HasVoted = hasVoted;

        return View(election);
    }

    // Cast a Vote
    [HttpPost]
    public async Task<IActionResult> CastVote(int candidateId, int electionId)
    {
        int userId = GetCurrentUserId();

        // Check if the user has already voted in this election
        bool hasVoted = await _context.Votes
            .AnyAsync(v => v.UserId == userId && v.ElectionId == electionId);

        if (hasVoted)
        {
            TempData["Error"] = "You have already voted in this election.";
            return RedirectToAction("Candidates", new { electionId });
        }

        // Create a new vote
        var vote = new Vote
        {
            UserId = userId,
            CandidateId = candidateId,
            ElectionId = electionId,
            VoteTime = DateTime.Now
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Your vote has been cast successfully!";
        return RedirectToAction("Candidates", new { electionId });
    }

    // Helper Methods
    private int GetCurrentUserId()
    {
        return int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
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
                _context.Elections.Update(election);
            }
        }
        _context.SaveChanges();
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