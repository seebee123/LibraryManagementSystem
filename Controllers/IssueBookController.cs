using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagementSystem.Controllers
{
    public class IssueBookController : Controller
    {
        private readonly AppDbContext _context;

        public IssueBookController(AppDbContext context)
        {
            _context = context;
        }

        // ======================
        // LIST + SEARCH + PAGINATION
        // ======================
        public IActionResult Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var data = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower();

                data = data.Where(i =>
                    i.Student != null &&
                    i.Book != null &&
                    (
                        i.Student.RollNo.ToLower().Contains(searchString) ||
                        i.Student.Name.ToLower().Contains(searchString) ||
                        i.Book.Title.ToLower().Contains(searchString) ||
                        i.Status.ToLower().Contains(searchString)
                    ));
            }

            var totalRecords = data.Count();

            var result = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(result);
        }

        // ======================
        // CREATE (GET)
        // ======================
        public IActionResult Create()
        {
            ViewBag.Students = _context.Students.ToList();
            ViewBag.Books = _context.Books.ToList();
            return View();
        }

        // ======================
        // CREATE (POST)
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IssueBook issue)
        {
            if (ModelState.IsValid)
            {
                issue.IssueDate = DateTime.Now;
                issue.Status = "Issued";
                issue.Fine = 0;

                _context.IssueBooks.Add(issue);
                _context.SaveChanges();

                TempData["Success"] = "Book issued successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Students = _context.Students.ToList();
            ViewBag.Books = _context.Books.ToList();

            return View(issue);
        }

        // ======================
        // DETAILS
        // ======================
        public IActionResult Details(int id)
        {
            var issue = _context.IssueBooks
                .Include(i => i.Student)
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
            var issue = _context.IssueBooks.Find(id);

            if (issue != null && issue.Status == "Issued")
            {
                issue.ReturnDate = DateTime.Now;
                issue.Status = "Returned";

                if (issue.ReturnDate > issue.DueDate)
                {
                    var daysLate = (issue.ReturnDate.Value - issue.DueDate).Days;
                    issue.Fine = daysLate * 10;
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
        // EXPORT TO EXCEL
        // ======================
        public IActionResult ExportExcel()
        {
            var issues = _context.IssueBooks
                .Include(i => i.Student)
                .Include(i => i.Book)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Issued Books");

                worksheet.Cell(1, 1).Value = "Student Name";
                worksheet.Cell(1, 2).Value = "Roll No";
                worksheet.Cell(1, 3).Value = "Book Title";
                worksheet.Cell(1, 4).Value = "Issue Date";
                worksheet.Cell(1, 5).Value = "Due Date";
                worksheet.Cell(1, 6).Value = "Return Date";
                worksheet.Cell(1, 7).Value = "Fine";
                worksheet.Cell(1, 8).Value = "Status";

                int row = 2;

                foreach (var i in issues)
                {
                    worksheet.Cell(row, 1).Value = i.Student?.Name;
                    worksheet.Cell(row, 2).Value = i.Student?.RollNo;
                    worksheet.Cell(row, 3).Value = i.Book?.Title;
                    worksheet.Cell(row, 4).Value = i.IssueDate.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 5).Value = i.DueDate.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 6).Value = i.ReturnDate.HasValue
                        ? i.ReturnDate.Value.ToString("dd-MM-yyyy")
                        : "-";
                    worksheet.Cell(row, 7).Value = i.Fine;
                    worksheet.Cell(row, 8).Value = i.Status;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "IssuedBooks.xlsx"
                    );
                }
            }
        }
    }
}