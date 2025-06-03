using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.WebApi.Data;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.Data.Repositories.Realisations;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Mapping.Users;
using UserService.WebApi.Services.Interfaces;
using UserService.WebApi.Services.Realisations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<UserServiseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAutoMapper(typeof(UserProfile));

builder.Services.AddSwaggerGen();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<UserServiseDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
}

app.MapControllers();

app.Run();
