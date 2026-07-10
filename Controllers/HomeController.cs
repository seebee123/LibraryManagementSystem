using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using System.Linq;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ===== LOGIN PROTECTION =====
            if (HttpContext.Session.GetString("User") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // ===== DASHBOARD STATS =====

            // Total Books
            ViewBag.TotalBooks = _context.Books.Count();

            // Total Students
            ViewBag.TotalStudents = _context.Students.Count();

            // Total Teachers
            ViewBag.TotalTeachers = _context.Teachers.Count();

            // Total Issued Books
            ViewBag.IssuedBooks = _context.IssueBooks
                .Count(x => x.Status == "Issued");

            // Student Issues
            ViewBag.StudentIssues = _context.IssueBooks
                .Count(x => x.IssueType == "Student"
                         && x.Status == "Issued");

            // Teacher Issues
            ViewBag.TeacherIssues = _context.IssueBooks
                .Count(x => x.IssueType == "Teacher"
                         && x.Status == "Issued");

            // Returned Books
            ViewBag.ReturnedBooks = _context.IssueBooks
                .Count(x => x.Status == "Returned");

            // NEW - Overdue Books
            ViewBag.OverdueBooks = _context.IssueBooks
                .Count(x =>
                    x.Status == "Issued" &&
                    x.DueDate < DateTime.Now);

            // Total Fine
            ViewBag.TotalFine = _context.IssueBooks
                .Sum(x => (decimal?)x.Fine) ?? 0;

            return View();
        }

        // ===== LOGOUT =====
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}