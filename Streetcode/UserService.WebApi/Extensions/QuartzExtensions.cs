using Quartz;
using UserService.WebApi.Jobs.Configurations;

namespace UserService.WebApi.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.ConfigureOptions<DeleteRevokedRefreshTokensJobConfiguration>();

        return services;
    }
}