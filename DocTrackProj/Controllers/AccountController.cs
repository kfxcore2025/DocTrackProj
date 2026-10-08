using DocTrackProj.Data;
using DocTrackProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocTrackProj.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AccountController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            ViewBag.Offices = await _db.Offices.OrderBy(o => o.OfficeName).ToListAsync();
            return View(new LoginViewModel());
        }

        // === SIGN IN FUNCTIONALITY ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewBag.Offices = await _db.Offices.OrderBy(o => o.OfficeName).ToListAsync();
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password && u.IsActive);
            if (user == null)
            {
                TempData["Error"] = "Invalid email or password.";
                return View(model);
            }

            // Save to session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Dashboard");
        }

        // === SIGN UP FUNCTIONALITY ===
        [HttpPost]
        public async Task<IActionResult> Register(string FullName, string Email, string Password, string Position, int OfficeId)
        {
            ViewBag.Offices = await _db.Offices.OrderBy(o => o.OfficeName).ToListAsync();

            if (await _db.Users.AnyAsync(u => u.Email == Email))
            {
                TempData["Error"] = "Email already exists.";
                return RedirectToAction("Login");
            }

            var newUser = new User
            {
                FullName = FullName,
                Email = Email,
                Password = Password, // For thesis only. In production use hashing!
                Position = Position,
                OfficeId = OfficeId,
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Account created! You can now Sign In.";
            return RedirectToAction("Login");
        }

        // === FORGOT PASSWORD FUNCTIONALITY ===
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user == null)
            {
                TempData["Error"] = "Email not found.";
                return RedirectToAction("Login");
            }

            // Simple reset for thesis - sets password to 123456
            user.Password = "123456";
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Password reset! Your new password is: 123456 (for {Email}). Please login and change it.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}