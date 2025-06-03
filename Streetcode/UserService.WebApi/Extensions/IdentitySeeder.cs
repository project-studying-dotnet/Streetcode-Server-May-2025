using Microsoft.AspNetCore.Identity;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Extensions;

public static class IdentitySeeder
{
    private const string IdentitySeedSection = "IdentitySeed";
    private const string RolesPath = $"{IdentitySeedSection}:Roles";
    private const string AdminSectionPath = $"{IdentitySeedSection}:DefaultAdmin";
    private const string AdminEmailKey = "Email";
    private const string AdminPasswordKey = "Password";

    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var roles = cfg.GetSection(RolesPath).Get<string[]>() ?? [];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminSection = cfg.GetSection(AdminSectionPath);
        var email = adminSection[AdminEmailKey];
        var password = adminSection[AdminPasswordKey];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Name = "Default",
            Surname = "Admin"
        };

        if (!(await userManager.CreateAsync(admin, password)).Succeeded)
            return;

        var adminRole = roles.FirstOrDefault(r =>
            r.StartsWith("Admin", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(adminRole))
            await userManager.AddToRoleAsync(admin, adminRole);
    }
}