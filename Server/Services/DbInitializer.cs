using DataAccess;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Server.Services;

public interface IDbInitializer
{
    void Initialize();
}

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DbInitializer> _logger;
    public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<DbInitializer> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            if (_db.Database.GetPendingMigrations().Any())
            {
                _db.Database.Migrate();
                Console.WriteLine("Pending migrations have been applied.");
                //_logger.LogInformation("");
            }
            else
                Console.WriteLine("No pending migrations");
        }
        catch (Exception)
        {
            //_logger.LogError("");
        }

        // Create 4 of test accounts
        var test1 = _db.Users.FirstOrDefault(u => u.Email == "test1@traceless.com");
        if (test1 == null)
        {
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "test1@traceless.com",
                Email = "test1@traceless.com",
                EmailConfirmed = true,
            }, "Test123@")
            .GetAwaiter().GetResult();
        }
        var test2 = _db.Users.FirstOrDefault(u => u.Email == "test2@traceless.com");
        if (test2 == null)
        {
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "test2@traceless.com",
                Email = "test2@traceless.com",
                EmailConfirmed = true,
            }, "Test123@")
            .GetAwaiter().GetResult();
        }
        var test3 = _db.Users.FirstOrDefault(u => u.Email == "test3@traceless.com");
        if (test3 == null)
        {
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "test3@traceless.com",
                Email = "test3@traceless.com",
                EmailConfirmed = true,
            }, "Test123@")
            .GetAwaiter().GetResult();
        }
        var test4 = _db.Users.FirstOrDefault(u => u.Email == "test4@traceless.com");
        if (test4 == null)
        {
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "test4@traceless.com",
                Email = "test4@traceless.com",
                EmailConfirmed = true,
            }, "Test123@")
            .GetAwaiter().GetResult();
        }
    }
}
