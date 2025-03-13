using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using VotingApp.Models;
using VotingApp.Data;

public class AuthController : Controller
{
    private readonly VotingDbContext _context;

    public AuthController(VotingDbContext context)
    {
        _context = context;
    }

    // Registration View
    public IActionResult Register()
    {
        return View();
    }

    // Handle Registration
    [HttpPost]
    public async Task<IActionResult> Register(User model, int roleId = 3) // Default Role: Voter (RoleId = 3)
    {
        if (ModelState.IsValid)
        {
            // Ensure Email and NIC are unique
            if (await _context.Users.AnyAsync(u => u.Email == model.Email || u.NIC == model.NIC))
            {
                ModelState.AddModelError("", "Email or NIC is already registered.");
                return View(model);
            }

            if (model.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("", "The password Length is insufficient");
                return View(model);
            }

            // Hash password before storing
            model.PasswordHash = HashPassword(model.PasswordHash);

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            // Assign Role to User (Ensure One-to-One Role Assignment)
            var existingUserRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == model.Id);
            if (existingUserRole == null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = model.Id,
                    RoleId = roleId // Default role (Voter)
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Login");
        }
        return View(model);
    }

    // Login View
    public IActionResult Login()
    {
        return View();
    }

    // Handle Login
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        // Retrieve User Role (Only One Role Per User)
        var userRole = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "Voter";

        // Store session data
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", userRole); // Store role as a string

        // Redirect based on role
        return userRole switch
        {
            "Admin" => RedirectToAction("Dashboard", "Admin"),
            "Candidate" => RedirectToAction("Dashboard", "Candidate"),
            _ => RedirectToAction("Elections", "Vote"), // Default for Voter
        };
    }

    // Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
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

    // Password Verification
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}