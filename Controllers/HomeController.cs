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
            // ===== LOGIN PROTECTION (IMPORTANT) =====
            if (HttpContext.Session.GetString("User") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // ===== DASHBOARD STATS =====
            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.TotalStudents = _context.Students.Count();

            ViewBag.IssuedBooks = _context.IssueBooks
                .Count(x => x.Status == "Issued");

            ViewBag.ReturnedBooks = _context.IssueBooks
                .Count(x => x.Status == "Returned");

            // SAFE FINE CALCULATION (avoid null crash)
            ViewBag.TotalFine = _context.IssueBooks
                .Sum(x => (decimal?)x.Fine) ?? 0;

            return View();
        }

        // OPTIONAL: LOGOUT FROM HOME
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}