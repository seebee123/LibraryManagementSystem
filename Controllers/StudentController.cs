using Microsoft.AspNetCore.Mvc; 
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================
        // LIST + SEARCH + PAGINATION
        // ==========================
        public IActionResult Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var students = _context.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                students = students.Where(s =>
                    s.Name.Contains(searchString) ||
                    s.RollNo.Contains(searchString) ||
                    s.Department.Contains(searchString) ||
                    s.Email.Contains(searchString));
            }

            var totalRecords = students.Count();

            var data = students
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
        public IActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Students.Add(student);
                _context.SaveChanges();

                TempData["Success"] = "Student added successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // ==========================
        // DETAILS
        // ==========================
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var student = _context.Students.FirstOrDefault(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // ==========================
        // EDIT (GET)
        // ==========================
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var student = _context.Students.Find(id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // ==========================
        // EDIT (POST)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Student student)
        {
            if (id != student.StudentId)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(student);
                _context.SaveChanges();

                TempData["Success"] = "Student updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // ==========================
        // DELETE (GET)
        // ==========================
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var student = _context.Students.FirstOrDefault(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // ==========================
        // DELETE (POST)
        // ==========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = _context.Students.Find(id);

            if (student != null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();

                TempData["Success"] = "Student deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // EXPORT STUDENTS TO EXCEL
        // ==========================
        public IActionResult ExportExcel()
        {
            var students = _context.Students.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Students");

                worksheet.Cell(1, 1).Value = "Roll No";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Department";
                worksheet.Cell(1, 4).Value = "Semester";
                worksheet.Cell(1, 5).Value = "Admission Year";
                worksheet.Cell(1, 6).Value = "Email";
                worksheet.Cell(1, 7).Value = "Phone";
                worksheet.Cell(1, 8).Value = "Address";

                int row = 2;

                foreach (var s in students)
                {
                    worksheet.Cell(row, 1).Value = s.RollNo;
                    worksheet.Cell(row, 2).Value = s.Name;
                    worksheet.Cell(row, 3).Value = s.Department;
                    worksheet.Cell(row, 4).Value = s.Semester;
                    worksheet.Cell(row, 5).Value = s.AdmissionYear;
                    worksheet.Cell(row, 6).Value = s.Email;
                    worksheet.Cell(row, 7).Value = s.Phone;
                    worksheet.Cell(row, 8).Value = s.Address;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Students.xlsx"
                    );
                }
            }
        }
    }
}