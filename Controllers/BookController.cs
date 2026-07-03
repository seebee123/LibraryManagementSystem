using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagementSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================
        // LIST ALL BOOKS + SEARCH + PAGINATION
        // ==========================
        public IActionResult Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString) ||
                    b.ISBN.Contains(searchString) ||
                    b.Category.Contains(searchString));
            }

            var totalRecords = books.Count();

            var data = books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(data);
        }

        // ==========================
        // CREATE (GET)
        // ==========================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================
        // CREATE (POST)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                _context.SaveChanges();

                TempData["Success"] = "Book added successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // ==========================
        // DETAILS
        // ==========================
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var book = _context.Books.FirstOrDefault(x => x.BookId == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // ==========================
        // EDIT (GET)
        // ==========================
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = _context.Books.Find(id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // ==========================
        // EDIT (POST)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Book book)
        {
            if (id != book.BookId)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(book);
                _context.SaveChanges();

                TempData["Success"] = "Book updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // ==========================
        // DELETE (GET)
        // ==========================
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = _context.Books.FirstOrDefault(x => x.BookId == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // ==========================
        // DELETE (POST)
        // ==========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var book = _context.Books.Find(id);

            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();

                TempData["Success"] = "Book deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // EXPORT BOOKS TO EXCEL
        // ==========================
        public IActionResult ExportExcel()
        {
            var books = _context.Books.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Books");

                worksheet.Cell(1, 1).Value = "Title";
                worksheet.Cell(1, 2).Value = "Author";
                worksheet.Cell(1, 3).Value = "ISBN";
                worksheet.Cell(1, 4).Value = "Category";
                worksheet.Cell(1, 5).Value = "Available Quantity";
                worksheet.Cell(1, 6).Value = "Status";

                int row = 2;

                foreach (var book in books)
                {
                    worksheet.Cell(row, 1).Value = book.Title;
                    worksheet.Cell(row, 2).Value = book.Author;
                    worksheet.Cell(row, 3).Value = book.ISBN;
                    worksheet.Cell(row, 4).Value = book.Category;
                    worksheet.Cell(row, 5).Value = book.AvailableQuantity;
                    worksheet.Cell(row, 6).Value = book.Status;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "LibraryBooks.xlsx");
                }
            }
        }
    }
}