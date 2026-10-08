using DocTrackProj.Data;
using DocTrackProj.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocTrackProj.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET LOGGED-IN USER OFFICE
        // =========================
        private int? GetCurrentOfficeId()
        {
            return HttpContext.Session.GetInt32("OfficeId");
        }

        private IActionResult? CheckUserSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");
            var officeId = HttpContext.Session.GetInt32("OfficeId");

            if (!userId.HasValue || role != "User" || !officeId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            return null;
        }

        // =========================
        // DASHBOARD
        // =========================
        public async Task<IActionResult> Dashboard()
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null)
            {
                return sessionCheck;
            }

            var officeId = GetCurrentOfficeId()!.Value;

            var officeDocuments = _context.Documents
                .Where(d => d.FromOfficeId == officeId || d.ToOfficeId == officeId);

            var totalDocuments = await officeDocuments.CountAsync();
            var pendingDocuments = await officeDocuments.CountAsync(d => d.Status == "Pending");
            var receivedDocuments = await officeDocuments.CountAsync(d => d.Status == "Received");
            var forSignatureDocuments = await officeDocuments.CountAsync(d => d.Status == "For Signature");

            var recentDocuments = await officeDocuments
                .Include(d => d.FromOffice)
                .Include(d => d.ToOffice)
                .OrderByDescending(d => d.DateSubmitted)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.PendingDocuments = pendingDocuments;
            ViewBag.ReceivedDocuments = receivedDocuments;
            ViewBag.ForSignatureDocuments = forSignatureDocuments;
            ViewBag.RecentDocuments = recentDocuments;

            return View();
        }

        // =========================
        // SUBMIT DOCUMENT - GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Submit()
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null)
            {
                return sessionCheck;
            }

            var officeId = GetCurrentOfficeId()!.Value;

            var currentOffice = await _context.Offices
                .FirstOrDefaultAsync(o => o.Id == officeId);

            ViewBag.CurrentOffice = currentOffice;

            var offices = await _context.Offices
                .Where(o => o.Id != officeId)
                .OrderBy(o => o.OfficeName)
                .ToListAsync();

            ViewBag.Offices = offices;

            return View();
        }

        // =========================
        // SUBMIT DOCUMENT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            string DocumentName,
            string ToOffice,
            string Reason,
            string Description)
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null) return sessionCheck;

            var currentOfficeId = GetCurrentOfficeId()!.Value;

            if (string.IsNullOrWhiteSpace(DocumentName))
            {
                TempData["Error"] = "Document name is required.";
                return RedirectToAction("Submit");
            }

            if (string.IsNullOrWhiteSpace(ToOffice))
            {
                TempData["Error"] = "Please select the receiving office.";
                return RedirectToAction("Submit");
            }

            var toOfficeEntity = await _context.Offices
                .FirstOrDefaultAsync(o => o.OfficeName == ToOffice);

            if (toOfficeEntity == null)
            {
                TempData["Error"] = "The selected receiving office does not exist.";
                return RedirectToAction("Submit");
            }

            if (toOfficeEntity.Id == currentOfficeId)
            {
                TempData["Error"] = "You cannot submit a document to the same office.";
                return RedirectToAction("Submit");
            }

            // Generate tracking code
            var lastId = await _context.UserDocuments
                .OrderByDescending(d => d.Id)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

            int nextNumber = (lastId ?? 0) + 1;
            string trackingCode = $"USR-DOC-{nextNumber:D6}";
            var transferKey = Guid.NewGuid();

            // 1. Save in UserDocuments
            var userDocument = new UserDocument
            {
                TransferKey = transferKey,
                TrackingCode = trackingCode,
                DocumentName = DocumentName,
                FromOfficeId = currentOfficeId,
                ToOfficeId = toOfficeEntity.Id,
                Status = "Pending",
                Reason = string.IsNullOrWhiteSpace(Reason) ? "For Approval" : Reason.Trim(),
                Description = Description ?? string.Empty,
                DateSubmitted = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            _context.UserDocuments.Add(userDocument);

            // 2. Also create AdminDocument so Admin can receive it
            //    (especially when To = Admin Office, but safe to always mirror)
            var adminDocument = new AdminDocument
            {
                TransferKey = transferKey,
                TrackingCode = trackingCode,
                DocumentName = DocumentName,
                FromOfficeId = currentOfficeId,
                ToOfficeId = toOfficeEntity.Id,
                Status = "Pending",
                Reason = string.IsNullOrWhiteSpace(Reason) ? "For Approval" : Reason.Trim(),
                Description = Description ?? string.Empty,
                DateSubmitted = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            _context.AdminDocuments.Add(adminDocument);

            await _context.SaveChangesAsync();

            TempData["TrackingCode"] = trackingCode;
            TempData["Success"] = true;
            return RedirectToAction("Submit");
        }

        // =========================
        // RECEIVE
        // =========================
        public async Task<IActionResult> Receive()
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null) return sessionCheck;

            var officeId = GetCurrentOfficeId()!.Value;

            // Only documents sent TO this user's office
            var documents = await _context.UserDocuments
                .Include(d => d.FromOffice)
                .Include(d => d.ToOffice)
                .Where(d => d.ToOfficeId == officeId && d.Status == "Pending")
                .OrderByDescending(d => d.DateSubmitted)
                .ToListAsync();

            ViewBag.NotificationCount = documents.Count;
            ViewBag.Notifications = documents.Take(10).ToList();

            return View(documents);
        }

        // =========================
        // MARK AS RECEIVED
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReceived(int id)
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null) return sessionCheck;

            var officeId = GetCurrentOfficeId()!.Value;

            var userDocument = await _context.UserDocuments
                .FirstOrDefaultAsync(d =>
                    d.Id == id &&
                    d.ToOfficeId == officeId &&
                    d.Status == "Pending");

            if (userDocument == null)
            {
                TempData["Error"] = "The document was not found or is not assigned to your office.";
                return RedirectToAction("Receive");
            }

            userDocument.Status = "Received";
            userDocument.DateUpdated = DateTime.Now;

            // Sync AdminDocument
            if (userDocument.TransferKey != Guid.Empty)
            {
                var adminDocument = await _context.AdminDocuments
                    .FirstOrDefaultAsync(d => d.TransferKey == userDocument.TransferKey);

                if (adminDocument != null)
                {
                    adminDocument.Status = "Received";
                    adminDocument.DateUpdated = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Document marked as received.";
            return RedirectToAction("Receive");
        }

        // =========================
        // MY DOCUMENTS
        // =========================
        public async Task<IActionResult> MyDocuments()
        {
            var officeId = GetCurrentOfficeId()!.Value;

            var documents = await _context.UserDocuments
                .Include(d => d.FromOffice)
                .Include(d => d.ToOffice)
                .Where(d => d.FromOfficeId == officeId || d.ToOfficeId == officeId)
                .OrderByDescending(d => d.DateSubmitted)
                .ToListAsync();

            return View(documents);  // sends data to cshtml
        }

        // =========================
        // DOCUMENT DETAILS
        // =========================
        public async Task<IActionResult> DocumentDetails(int id)
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null)
            {
                return sessionCheck;
            }

            var officeId = GetCurrentOfficeId()!.Value;

            var document = await _context.UserDocuments   // change to Documents if you still use that table
                .Include(d => d.FromOffice)
                .Include(d => d.ToOffice)
                .FirstOrDefaultAsync(d => d.Id == id &&
                    (d.FromOfficeId == officeId || d.ToOfficeId == officeId));

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // =========================
        // SETTINGS
        // =========================
        public IActionResult Settings()
        {
            var sessionCheck = CheckUserSession();
            if (sessionCheck != null)
            {
                return sessionCheck;
            }

            return View();
        }
    }
}