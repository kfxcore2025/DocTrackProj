using DocTrackProj.Data;
using DocTrackProj.Models;
using DocTrackProj.Services;
using Microsoft.EntityFrameworkCore;

namespace DocTrackProj
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    db.Database.Migrate();

                    // Seed only if Users table exists
                    if (db.Database.CanConnect() && db.Users.Any())
                    {
                        // table exists, do nothing
                    }

                    var adminUser = db.Users.FirstOrDefault(u => u.Email == "admin@ctu.edu.ph");
                    if (adminUser == null)
                    {
                        db.Users.Add(new User
                        {
                            FullName = "System Administrator",
                            Email = "admin@ctu.edu.ph",
                            Password = "admin123",
                            Role = "Admin",
                            OfficeId = null,
                            Position = "Administrator",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                    }

                    var regularUser = db.Users.FirstOrDefault(u => u.Email == "user@ctu.edu.ph");
                    if (regularUser == null)
                    {
                        db.Users.Add(new User
                        {
                            FullName = "System User",
                            Email = "user@ctu.edu.ph",
                            Password = "User@2026",
                            Role = "User",
                            OfficeId = null,
                            Position = "Staff",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Seed error: " + ex.Message);
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");
            app.Run();
        }
    }
}