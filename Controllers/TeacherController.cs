using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagementSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================
        // LIST + SEARCH + PAGINATION
        // ==========================
        public IActionResult Index(string searchString, int page = 1)
        {
            int pageSize = 10;

            var teachers = _context.Teachers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                teachers = teachers.Where(t =>
                    t.EmployeeId.Contains(searchString) ||
                    t.Name.Contains(searchString) ||
                    t.Department.Contains(searchString) ||
                    t.Designation.Contains(searchString) ||
                    t.Email.Contains(searchString));
            }

            var totalRecords = teachers.Count();

            var data = teachers
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
        public IActionResult Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Teachers.Add(teacher);
                _context.SaveChanges();

                TempData["Success"] = "Teacher added successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // ==========================
        // DETAILS
        // ==========================
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var teacher = _context.Teachers
                .FirstOrDefault(x => x.TeacherId == id);

            if (teacher == null)
                return NotFound();

            return View(teacher);
        }

        // ==========================
        // EDIT (GET)
        // ==========================
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var teacher = _context.Teachers.Find(id);

            if (teacher == null)
                return NotFound();

            return View(teacher);
        }

        // ==========================
        // EDIT (POST)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Teacher teacher)
        {
            if (id != teacher.TeacherId)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(teacher);
                _context.SaveChanges();

                TempData["Success"] = "Teacher updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }
        // ==========================
        // DELETE (GET)
        // ==========================
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var teacher = _context.Teachers
                .FirstOrDefault(x => x.TeacherId == id);

            if (teacher == null)
                return NotFound();

            return View(teacher);
        }

        // ==========================
        // DELETE (POST)
        // ==========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var teacher = _context.Teachers.Find(id);

            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                _context.SaveChanges();

                TempData["Success"] = "Teacher deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================
        // EXPORT TEACHERS TO EXCEL
        // ==========================
        public IActionResult ExportExcel()
        {
            var teachers = _context.Teachers.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Teachers");

                worksheet.Cell(1, 1).Value = "Employee ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Department";
                worksheet.Cell(1, 4).Value = "Designation";
                worksheet.Cell(1, 5).Value = "Email";
                worksheet.Cell(1, 6).Value = "Phone";
                worksheet.Cell(1, 7).Value = "Address";
                worksheet.Cell(1, 8).Value = "Status";

                int row = 2;

                foreach (var t in teachers)
                {
                    worksheet.Cell(row, 1).Value = t.EmployeeId;
                    worksheet.Cell(row, 2).Value = t.Name;
                    worksheet.Cell(row, 3).Value = t.Department;
                    worksheet.Cell(row, 4).Value = t.Designation;
                    worksheet.Cell(row, 5).Value = t.Email;
                    worksheet.Cell(row, 6).Value = t.Phone;
                    worksheet.Cell(row, 7).Value = t.Address;
                    worksheet.Cell(row, 8).Value = t.Status;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Teachers.xlsx"
                    );
                }
            }
        }
    }
}