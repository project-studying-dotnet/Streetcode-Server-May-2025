using Hangfire;
using Microsoft.Azure.Amqp.Framing;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.WebApi.Extensions;
using Streetcode.WebApi.Middlewares;
using Streetcode.WebApi.Services.MessageConsumers;
using Streetcode.WebApi.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureApplication();

builder.Services.AddCommunication(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddSwaggerServices();
builder.Services.AddCustomServices();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddAzureServiceBusIntegration(builder.Configuration);
builder.Services.AddHostedService<UserRegistrationBackgroundService>();
builder.Services.ConfigureBlob(builder);
builder.Services.ConfigurePayment(builder);
builder.Services.ConfigureInstagram(builder);
builder.Services.ConfigureSerilog(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local") || app.Environment.IsEnvironment("Experimental"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
}
else
{
    app.UseHsts();
}

await app.ApplyMigrations();

// await app.SeedDataAsync(); // uncomment for seeding data in local
app.UseCors();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

app.UseHangfireDashboard("/dash");

if (app.Environment.EnvironmentName != "Local")
{
    BackgroundJob.Schedule<WebParsingUtils>(
    wp => wp.ParseZipFileFromWebAsync(), TimeSpan.FromMinutes(1));

    RecurringJob.AddOrUpdate(
        "web-parsing-monthly",
        (WebParsingUtils wp) => wp.ParseZipFileFromWebAsync(),
        Cron.Monthly);

    RecurringJob.AddOrUpdate(
        "blob-cleanup-monthly",
        (BlobService b) => b.CleanBlobStorage(),
        Cron.Monthly);
}

app.MapControllers();

app.Run();

public partial class Program
{
}