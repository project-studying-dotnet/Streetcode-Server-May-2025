using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.WebApi.Data;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.Data.Repositories.Realisations;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Extensions;
using UserService.WebApi.Mapping.Users;
using UserService.WebApi.Services.Interfaces;
using UserService.WebApi.Services.Realisations;
using UserService.WebApi.Validators.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCommunication(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<UserServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddQuartzJobs();

builder.Services.AddScoped<IUserRegistrationPublisher, UserRegistrationPublisher>();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDTOValidator>();

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
    .AddEntityFrameworkStores<UserServiceDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddSwaggerWithJwt();

builder.Services.AddAzureServiceBusIntegration(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
}

await app.Services.SeedIdentityAsync(); // uncomment for seeding data

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();