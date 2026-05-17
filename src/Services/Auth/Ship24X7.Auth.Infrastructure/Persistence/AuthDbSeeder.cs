using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence;

/// <summary>
/// Seeds the Auth database with default roles and one user per role for development.
/// All seed users have EmailVerified=true and IsActive=true so they can log in immediately.
/// Enforces single System_Admin constraint on every startup.
///
/// Seed credentials follow the pattern:
///   Email    : {role-slug}@ship24x7.local
///   Password : Ship24X7@{RoleSlug}1  (e.g. Ship24X7@SystemAdmin1)
/// </summary>
public static class AuthDbSeeder
{
    // -------------------------------------------------------------------------
    // Seed data definitions — single place to update roles and users
    // -------------------------------------------------------------------------

    private static readonly List<(string Name, string Description)> SeedRoles = new()
    {
        ("System_Admin",      "System Administrator with full access"),
        ("Admin_User",        "Administrative user with management access"),
        ("Operations_Staff",  "Operations staff for shipment management"),
        ("Support_Staff",     "Customer support staff"),
        ("Manager",           "General manager role"),
        ("Area_Manager",      "Area manager for regional operations"),
        ("Regional_Manager",  "Regional manager"),
        ("Hub_Manager",       "Hub manager for distribution centers"),
        ("Delivery_Agent",    "Delivery agent for last-mile delivery"),
        ("Customer",          "Customer role for end users"),
    };

    private static readonly List<(string Email, string FullName, string Phone, string Password, string RoleName)> SeedUsers = new()
    {
        ("system-admin@ship24x7.local",     "System Administrator",  "+91-9000000001", "Ship24X7@SystemAdmin1",     "System_Admin"),
        ("admin@ship24x7.local",            "Admin User",            "+91-9000000002", "Ship24X7@AdminUser1",       "Admin_User"),
        ("operations@ship24x7.local",       "Operations Staff",      "+91-9000000003", "Ship24X7@Operations1",      "Operations_Staff"),
        ("support@ship24x7.local",          "Support Staff",         "+91-9000000004", "Ship24X7@Support1",         "Support_Staff"),
        ("manager@ship24x7.local",          "Manager",               "+91-9000000005", "Ship24X7@Manager1",         "Manager"),
        ("area-manager@ship24x7.local",     "Area Manager",          "+91-9000000006", "Ship24X7@AreaManager1",     "Area_Manager"),
        ("regional-manager@ship24x7.local", "Regional Manager",      "+91-9000000007", "Ship24X7@RegionalMgr1",     "Regional_Manager"),
        ("hub-manager@ship24x7.local",      "Hub Manager",           "+91-9000000008", "Ship24X7@HubManager1",      "Hub_Manager"),
        ("delivery-agent@ship24x7.local",   "Delivery Agent",        "+91-9000000009", "Ship24X7@DeliveryAgent1",   "Delivery_Agent"),
        ("customer@ship24x7.local",         "Customer User",         "+91-9000000010", "Ship24X7@Customer1",        "Customer"),
    };

    // -------------------------------------------------------------------------

    public static async Task SeedAsync(AuthDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(context);
        await SeedUsersAsync(context);
        EnforceSystemAdminConstraint(context);
    }

    private static async Task SeedRolesAsync(AuthDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var roles = SeedRoles.Select(r => new Role
        {
            Id          = Guid.NewGuid(),
            Name        = r.Name,
            Description = r.Description,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        }).ToList();

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AuthDbContext context)
    {
        var hasher = new PasswordHasher<User>();

        foreach (var (email, fullName, phone, password, roleName) in SeedUsers)
        {
            if (await context.Users.AnyAsync(u => u.Email == email))
                continue;

            var user = new User
            {
                Id            = Guid.NewGuid(),
                Email         = email,
                FullName      = fullName,
                PhoneNumber   = phone,
                EmailVerified = true,
                PhoneVerified = false,
                IsActive      = true,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };

            user.PasswordHash = hasher.HashPassword(user, password);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    Id        = Guid.NewGuid(),
                    UserId    = user.Id,
                    RoleId    = role.Id,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }
    }

    private static void EnforceSystemAdminConstraint(AuthDbContext context)
    {
        var systemAdminRoleId = context.Roles
            .Where(r => r.Name == "System_Admin")
            .Select(r => r.Id)
            .FirstOrDefault();

        if (systemAdminRoleId == Guid.Empty) return;

        var count = context.UserRoles.Count(ur => ur.RoleId == systemAdminRoleId);

        if (count > 1)
            throw new InvalidOperationException(
                "CRITICAL: Multiple System_Admin users detected. " +
                "Only one System Admin is allowed. Restore from backup.");
    }
}
