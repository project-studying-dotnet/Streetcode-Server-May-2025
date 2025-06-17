using Hangfire;
using Hangfire.SqlServer;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireServerWithSqlStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? hangfireConnectionString = configuration
                                              .GetConnectionString("HangfireConnection");

        services.AddHangfire(cfg => cfg
            .UseSqlServerStorage(
                hangfireConnectionString,
                new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                }));

        services.AddHangfireServer();

        return services;
    }

    public static IApplicationBuilder UseHangfireDashboardAndScheduler(
    this IApplicationBuilder app,
    IConfiguration configuration)
    {
        app.UseHangfireDashboard();

        var recurringJobsSection = configuration.GetSection("Hangfire:RecurringJobs");

        foreach (var jobSection in recurringJobsSection.GetChildren())
        {
            var jobKey = jobSection.Key;
            var jobId = jobSection.GetValue<string>("JobId");
            var cronSchedule = jobSection.GetValue<string>("Cron");

            if (string.IsNullOrWhiteSpace(jobId) || string.IsNullOrWhiteSpace(cronSchedule))
            {
                continue;
            }

            switch (jobKey)
            {
                case "RevokeExpiredTokens":
                    RecurringJob.AddOrUpdate<ITokenService>(
                        recurringJobId: jobId,
                        methodCall: svc => svc
                            .RevokeExpiredRefreshTokensAsync(CancellationToken.None),
                        cronExpression: cronSchedule
                    ); 
                    break;
            }
        }

        return app;
    }
}