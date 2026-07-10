using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagementSystem.Controllers
{
    public class IssueBookController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public IssueBookController(
            AppDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        // ======================
        // LIST + SEARCH + PAGINATION
        // ======================

        public IActionResult Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var data = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Teacher)
                .Include(i => i.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower();

                data = data.Where(i =>

                    (i.Student != null &&
                    (
                        i.Student.RollNo.ToLower().Contains(searchString) ||
                        i.Student.Name.ToLower().Contains(searchString)
                    ))

                    ||

                    (i.Teacher != null &&
                    (
                        i.Teacher.EmployeeId.ToLower().Contains(searchString) ||
                        i.Teacher.Name.ToLower().Contains(searchString)
                    ))

                    ||

                    (i.Book != null &&
                     i.Book.Title.ToLower().Contains(searchString))

                    ||

                    i.Status.ToLower().Contains(searchString)

                    ||

                    i.IssueType.ToLower().Contains(searchString)
                );
            }

            var totalRecords = data.Count();

            var result = data
                .OrderByDescending(x => x.IssueId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(result);
        }
        // ======================
        // OVERDUE BOOKS
        // ======================
        public IActionResult Overdue(string searchString, int page = 1)
        {
            int pageSize = 10;

            var data = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Teacher)
                .Include(i => i.Book)
                .Where(i =>
                    i.Status == "Issued" &&
                    i.DueDate < DateTime.Now)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower();

                data = data.Where(i =>

                    (i.Student != null &&
                    (
                        i.Student.RollNo.ToLower().Contains(searchString) ||
                        i.Student.Name.ToLower().Contains(searchString)
                    ))

                    ||

                    (i.Teacher != null &&
                    (
                        i.Teacher.EmployeeId.ToLower().Contains(searchString) ||
                        i.Teacher.Name.ToLower().Contains(searchString)
                    ))

                    ||

                    (i.Book != null &&
                        i.Book.Title.ToLower().Contains(searchString))
                );
            }

            var totalRecords = data.Count();

            var result = data
                .OrderBy(i => i.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(result);
        }
        // ======================
        // CREATE (GET)
        // ======================
        public IActionResult Create()
        {
            ViewBag.Students = _context.Students.ToList();
            ViewBag.Teachers = _context.Teachers.ToList();
            ViewBag.Books = _context.Books
    .Where(b => b.AvailableQuantity > 0)
    .OrderBy(b => b.BookCode)
    .ToList();
            return View();
        }

        // ======================
        // CREATE (POST)
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IssueBook issue)
        {
            if (issue.IssueType == "Student")
            {
                issue.TeacherId = null;

                if (issue.StudentId == null)
                {
                    ModelState.AddModelError(
                        "StudentId",
                        "Please select a student."
                    );
                }
            }
            else
            {
                issue.StudentId = null;

                if (issue.TeacherId == null)
                {
                    ModelState.AddModelError(
                        "TeacherId",
                        "Please select a teacher."
                    );
                }
            }

            if (ModelState.IsValid)
            {
                var book = _context.Books.FirstOrDefault(b => b.BookId == issue.BookId);

                if (book == null)
                {
                    TempData["Error"] = "Book not found.";
                }
                else if (book.AvailableQuantity <= 0)
                {
                    TempData["Error"] = "This book is currently not available.";
                }
                else
                {
                    // Reduce Available Quantity
                    book.AvailableQuantity--;

                    if (book.AvailableQuantity == 0)
                    {
                        book.Status = "Unavailable";
                    }

                    issue.Status = "Issued";
                    issue.Fine = 0;

                    _context.IssueBooks.Add(issue);
                    _context.SaveChanges();

                    TempData["Success"] = "Book issued successfully!";

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Students = _context.Students.ToList();
            ViewBag.Teachers = _context.Teachers.ToList();
            ViewBag.Books = _context.Books
    .Where(b => b.AvailableQuantity > 0)
    .OrderBy(b => b.BookCode)
    .ToList();

            return View(issue);
        }
        // ======================
        // DETAILS
        // ======================
        public IActionResult Details(int id)
        {
            var issue = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Teacher)
                .Include(i => i.Book)
                .FirstOrDefault(i => i.IssueId == id);

            if (issue == null)
                return NotFound();

            return View(issue);
        }

        // ======================
        // RETURN BOOK
        // ======================
        public IActionResult Return(int id)
        {
            var issue = _context.IssueBooks
                .FirstOrDefault(i => i.IssueId == id);

            if (issue != null && issue.Status == "Issued")
            {
                issue.ReturnDate = DateTime.Now;
                issue.Status = "Returned";
                var book = _context.Books.FirstOrDefault(b => b.BookId == issue.BookId);

                if (book != null)
                {
                    book.AvailableQuantity++;

                    if (book.AvailableQuantity > 0)
                    {
                        book.Status = "Available";
                    }
                }
                if (issue.ReturnDate > issue.DueDate)
                {
                    var daysLate = (issue.ReturnDate.Value - issue.DueDate).Days;

                    if (string.Equals(issue.IssueType, "Teacher", StringComparison.OrdinalIgnoreCase))
                    {
                        issue.Fine = daysLate * 100;   // ₹100 per day
                    }
                    else
                    {
                        issue.Fine = daysLate * 10;    // ₹10 per day
                    }
                }

                _context.SaveChanges();
            }

            TempData["Success"] = "Book returned successfully!";

            return RedirectToAction(nameof(Index));
        }
        // ======================
        // DELETE
        // ======================
        public IActionResult Delete(int id)
        {
            var issue = _context.IssueBooks.Find(id);

            if (issue != null)
            {
                _context.IssueBooks.Remove(issue);
                _context.SaveChanges();
            }

            TempData["Success"] = "Record deleted successfully!";

            return RedirectToAction(nameof(Index));
        }


        // ======================
        // SEND EMAIL REMINDER
        // ======================
        public async Task<IActionResult> SendReminder(int id)
        {
            var issue = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Teacher)
                .Include(i => i.Book)
                .FirstOrDefault(i => i.IssueId == id);

            if (issue == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction(nameof(Overdue));
            }

            string email = "";
            string name = "";

            if (issue.IssueType == "Student")
            {
                email = issue.Student?.Email ?? "";
                name = issue.Student?.Name ?? "";
            }
            else
            {
                email = issue.Teacher?.Email ?? "";
                name = issue.Teacher?.Name ?? "";
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email address not found.";
                return RedirectToAction(nameof(Overdue));
            }

            string subject = "Library Book Due Reminder";

            string body = $@"
        <h2>Hello {name},</h2>

        <p>This is a reminder that your library book is overdue.</p>

        <hr>

        <p><b>Book:</b> {issue.Book?.Title}</p>
        <p><b>Due Date:</b> {issue.DueDate:dd-MM-yyyy}</p>

        <hr>

        <p>Please return the book as soon as possible to avoid additional fines.</p>

        <br>

        <p>Regards,<br><b>Library Management System</b></p>";

            await _emailService.SendEmailAsync(email, subject, body);

            TempData["Success"] = "Reminder email sent successfully.";

            return RedirectToAction(nameof(Overdue));
        }

        // ======================
        // EXPORT TO EXCEL
        // ======================
        public IActionResult ExportExcel()
        {
            var issues = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Teacher)
                .Include(i => i.Book)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Issued Books");

                worksheet.Cell(1, 1).Value = "Issue Type";
                worksheet.Cell(1, 2).Value = "Issued To";
                worksheet.Cell(1, 3).Value = "ID";
                worksheet.Cell(1, 4).Value = "Book";
                worksheet.Cell(1, 5).Value = "Issue Date";
                worksheet.Cell(1, 6).Value = "Due Date";
                worksheet.Cell(1, 7).Value = "Return Date";
                worksheet.Cell(1, 8).Value = "Fine";
                worksheet.Cell(1, 9).Value = "Status";

                int row = 2;

                foreach (var i in issues)
                {
                    worksheet.Cell(row, 1).Value = i.IssueType;

                    if (string.Equals(i.IssueType, "Student", StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cell(row, 2).Value = i.Student?.Name;
                        worksheet.Cell(row, 3).Value = i.Student?.RollNo;
                    }
                    else
                    {
                        worksheet.Cell(row, 2).Value = i.Teacher?.Name;
                        worksheet.Cell(row, 3).Value = i.Teacher?.EmployeeId;
                    }

                    worksheet.Cell(row, 4).Value = i.Book?.Title;
                    worksheet.Cell(row, 5).Value = i.IssueDate.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 6).Value = i.DueDate.ToString("dd-MM-yyyy");

                    worksheet.Cell(row, 7).Value = i.ReturnDate.HasValue
                        ? i.ReturnDate.Value.ToString("dd-MM-yyyy")
                        : "-";

                    worksheet.Cell(row, 8).Value = i.Fine;
                    worksheet.Cell(row, 9).Value = i.Status;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "IssuedBooks.xlsx");
                }
            }
        }
    }
}
