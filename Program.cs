using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================
            // SERVICES
            // =========================
            builder.Services.AddControllersWithViews();

            // DB CONNECTION
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ✅ SESSION ENABLE (IMPORTANT FIX)
            builder.Services.AddSession();

            var app = builder.Build();

            // =========================
            // MIDDLEWARE PIPELINE
            // =========================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ✅ SESSION MIDDLEWARE (IMPORTANT FIX)
            app.UseSession();

            app.UseAuthorization();

            // ROUTES
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}