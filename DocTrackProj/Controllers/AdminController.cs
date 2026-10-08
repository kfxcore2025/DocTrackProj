using DocTrackProj.Data;
using DocTrackProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DocTrackProj.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // FIX FOR 404 - ADD THIS
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // =====================================================
        // HELPER: ADMIN DOCUMENTS
        // =====================================================
        private IQueryable<AdminDocument> GetAdminDocuments()
        {
            return _context.AdminDocuments.AsQueryable();
        }

        // =====================================================
        // HELPER: LOAD NOTIFICATIONS
        // =====================================================
        private async Task LoadNotificationData()
        {
            string currentOffice = "Admin Office";

            var pendingDocuments = await _context.AdminDocuments
               .Include(d => d.ToOffice)
               .CountAsync(d => d.Status == "Pending"
                              && d.ToOffice != null
                              && d.ToOffice.OfficeName == currentOffice);

            var notifications = await _context.AdminDocuments
               .Include(d => d.FromOffice)
               .Include(d => d.ToOffice)
               .Where(d => d.Status == "Pending"
                         && d.ToOffice != null
                         && d.ToOffice.OfficeName == currentOffice)
               .OrderByDescending(d => d.DateSubmitted)
               .Take(10)
               .ToListAsync();

            ViewBag.PendingDocuments = pendingDocuments;
            ViewBag.NotificationCount = pendingDocuments;
            ViewBag.Notifications = notifications;
        }

        // =====================================================
        // HELPER: GET NEXT ADMIN TRACKING CODE
        // =====================================================
        private async Task<string> GenerateAdminTrackingCode()
        {
            var lastDocumentId = await _context.AdminDocuments
               .OrderByDescending(d => d.Id)
               .Select(d => (int?)d.Id)
               .FirstOrDefaultAsync();

            var nextNumber = (lastDocumentId ?? 0) + 1;
            return $"ADM-DOC-{nextNumber:D6}";
        }

        // =====================================================
        // DASHBOARD
        // =====================================================
        public async Task<IActionResult> Dashboard()
        {
            var adminDocuments = GetAdminDocuments();
            var totalDocuments = await adminDocuments.CountAsync();
            var pendingDocuments = await adminDocuments.CountAsync(d => d.Status == "Pending");
            var approvedDocuments = await adminDocuments.CountAsync(d => d.Status == "Approved" || d.Status == "Signed");
            var forSignatureDocuments = await adminDocuments.CountAsync(d => d.Status == "For Signature" || d.Reason == "For Signature");

            var recentDocuments = await adminDocuments
               .Include(d => d.FromOffice)
               .Include(d => d.ToOffice)
               .OrderByDescending(d => d.DateSubmitted)
               .Take(5)
               .ToListAsync();

            var notifications = await adminDocuments
               .Include(d => d.FromOffice)
               .Include(d => d.ToOffice)
               .Where(d => d.Status == "Pending" || d.Status == "For Signature" || d.Reason == "For Signature")
               .OrderByDescending(d => d.DateSubmitted)
               .Take(10)
               .ToListAsync();

            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.PendingDocuments = pendingDocuments;
            ViewBag.ApprovedDocuments = approvedDocuments;
            ViewBag.ForSignatureDocuments = forSignatureDocuments;
            ViewBag.NotificationCount = pendingDocuments + forSignatureDocuments;
            ViewBag.Notifications = notifications;
            ViewBag.RecentDocuments = recentDocuments;

            return View();
        }

        // =====================================================
        // RECEIVE
        // =====================================================
        public async Task<IActionResult> Receive()
        {
            string currentOffice = "Admin Office";
            var documents = await _context.AdminDocuments
               .Include(d => d.FromOffice)
               .Include(d => d.ToOffice)
               .Where(d => d.Status == "Pending" && d.ToOffice != null && d.ToOffice.OfficeName == currentOffice)
               .OrderByDescending(d => d.DateSubmitted)
               .ToListAsync();

            await LoadNotificationData();
            return View(documents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReceived(int id)
        {
            string currentOffice = "Admin Office";
            var adminDocument = await _context.AdminDocuments.Include(d => d.ToOffice).FirstOrDefaultAsync(d => d.Id == id);
            if (adminDocument == null) return NotFound();

            if (adminDocument.Status != "Pending" || adminDocument.ToOffice?.OfficeName != currentOffice)
            {
                TempData["Error"] = "You cannot receive this document.";
                return RedirectToAction(nameof(Receive));
            }

            adminDocument.Status = "Received";
            adminDocument.DateUpdated = DateTime.Now;

            if (adminDocument.TransferKey != Guid.Empty)
            {
                var userDocument = await _context.UserDocuments.FirstOrDefaultAsync(d => d.TransferKey == adminDocument.TransferKey);
                if (userDocument != null)
                {
                    userDocument.Status = "Received";
                    userDocument.DateUpdated = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Document marked as received successfully.";
            return RedirectToAction(nameof(Receive));
        }

        [HttpGet]
        public async Task<IActionResult> SubDoc()
        {
            await LoadNotificationData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubDoc(string DocumentName, string FromOffice, string ToOffice, string Reason, string AssignedTo, string Description)
        {
            if (string.IsNullOrWhiteSpace(DocumentName) || string.IsNullOrWhiteSpace(FromOffice) || string.IsNullOrWhiteSpace(ToOffice))
            {
                TempData["Error"] = "Document name, From Department, and To Department are required.";
                return RedirectToAction(nameof(SubDoc));
            }

            var fromOfficeEntity = await _context.Offices.FirstOrDefaultAsync(o => o.OfficeName == FromOffice);
            var toOfficeEntity = await _context.Offices.FirstOrDefaultAsync(o => o.OfficeName == ToOffice);

            if (fromOfficeEntity == null || toOfficeEntity == null)
            {
                TempData["Error"] = "The selected department is invalid.";
                return RedirectToAction(nameof(SubDoc));
            }

            var trackingCode = await GenerateAdminTrackingCode();
            var transferKey = Guid.NewGuid();

            var adminDocument = new AdminDocument
            {
                TransferKey = transferKey,
                TrackingCode = trackingCode,
                DocumentName = DocumentName,
                FromOfficeId = fromOfficeEntity.Id,
                ToOfficeId = toOfficeEntity.Id,
                Status = "Pending",
                Reason = string.IsNullOrWhiteSpace(Reason) ? "For Approval" : Reason,
                Description = Description ?? string.Empty,
                DateSubmitted = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            _context.AdminDocuments.Add(adminDocument);

            if (toOfficeEntity.OfficeName != "Admin Office")
            {
                var userDocument = new UserDocument
                {
                    TransferKey = transferKey,
                    TrackingCode = trackingCode,
                    DocumentName = DocumentName,
                    FromOfficeId = fromOfficeEntity.Id,
                    ToOfficeId = toOfficeEntity.Id,
                    Status = "Pending",
                    Reason = string.IsNullOrWhiteSpace(Reason) ? "For Approval" : Reason,
                    Description = Description ?? string.Empty,
                    DateSubmitted = DateTime.Now,
                    DateUpdated = DateTime.Now
                };
                _context.UserDocuments.Add(userDocument);
            }

            await _context.SaveChangesAsync();
            TempData["TrackingCode"] = trackingCode;
            TempData["Success"] = true;
            return RedirectToAction(nameof(SubDoc));
        }

        public async Task<IActionResult> HistoryLogs()
        {
            var documents = await _context.AdminDocuments.Include(d => d.FromOffice).Include(d => d.ToOffice).OrderByDescending(d => d.DateSubmitted).ToListAsync();
            await LoadNotificationData();
            return View(documents);
        }

        public async Task<IActionResult> Audit()
        {
            var documents = await _context.AdminDocuments.Include(d => d.FromOffice).Include(d => d.ToOffice).OrderByDescending(d => d.DateSubmitted).ToListAsync();
            await LoadNotificationData();
            return View(documents);
        }

        public async Task<IActionResult> DocumentDetails(int id)
        {
            var document = await _context.AdminDocuments.Include(d => d.FromOffice).Include(d => d.ToOffice).FirstOrDefaultAsync(d => d.Id == id);
            if (document == null) return NotFound();
            await LoadNotificationData();
            return View(document);
        }

        public async Task<IActionResult> Manage()
        {
            var viewModel = new ManageViewModel
            {
                Users = await _context.Users.Include(u => u.Office).OrderByDescending(u => u.CreatedAt).ToListAsync(),
                Offices = await _context.Offices.OrderBy(o => o.OfficeName).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User model, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Password is required.";
                return RedirectToAction(nameof(Manage));
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                TempData["Error"] = "This email is already registered.";
                return RedirectToAction(nameof(Manage));
            }
            model.Password = password;
            model.CreatedAt = DateTime.Now;
            model.IsActive = true;
            _context.Users.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int Id, string FullName, string Email, string Role, string? newPassword, bool IsActive = false)
        {
            var user = await _context.Users.FindAsync(Id);
            if (user == null) return NotFound();
            if (await _context.Users.AnyAsync(u => u.Email == Email && u.Id != Id))
            {
                TempData["Error"] = "This email is already used by another user.";
                return RedirectToAction(nameof(Manage));
            }
            user.FullName = FullName;
            user.Email = Email;
            user.Role = Role;
            user.IsActive = IsActive;
            if (!string.IsNullOrWhiteSpace(newPassword)) user.Password = newPassword;
            await _context.SaveChangesAsync();
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = user.IsActive ? "User activated." : "User deactivated.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffice(Office model)
        {
            if (string.IsNullOrWhiteSpace(model.OfficeName))
            {
                TempData["Error"] = "Office name is required.";
                return RedirectToAction(nameof(Manage));
            }
            model.IsActive = true;
            _context.Offices.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Office created successfully.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOffice(int Id, string OfficeName, string RoomNumber, bool IsActive = false)
        {
            var office = await _context.Offices.FindAsync(Id);
            if (office == null) return NotFound();
            office.OfficeName = OfficeName;
            office.RoomNumber = RoomNumber ?? string.Empty;
            office.IsActive = IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Office updated successfully.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleOfficeActive(int id)
        {
            var office = await _context.Offices.FindAsync(id);
            if (office == null) return NotFound();
            office.IsActive = !office.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = office.IsActive ? "Office activated." : "Office deactivated.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            await LoadNotificationData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            // FIXED: was admin@ctu.db
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@ctu.edu.ph");
            if (currentUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Settings));
            }
            if (currentUser.Password != CurrentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Settings));
            }
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(Settings));
            }
            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 4)
            {
                TempData["Error"] = "New password must be at least 4 characters.";
                return RedirectToAction(nameof(Settings));
            }
            currentUser.Password = NewPassword;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction(nameof(Settings));
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "DocTraCodeSalt2024");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}