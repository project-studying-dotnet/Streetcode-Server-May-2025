using Quartz;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Jobs;

[DisallowConcurrentExecution]
public class DeleteRevokedRefreshTokensJob : IJob
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<DeleteRevokedRefreshTokensJob> _logger;

    public DeleteRevokedRefreshTokensJob(
        ITokenService tokenService,
        ILogger<DeleteRevokedRefreshTokensJob> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobName = context.JobDetail.Key.Name;
        _logger.LogInformation($"Quartz job '{jobName}' starting at {DateTime.UtcNow}");

        var result = await _tokenService.DeleteRevokedRefreshTokensAsync(context.CancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation($"Quartz job '{jobName}' deleted {result.Value} revoked tokens at {DateTime.UtcNow}");
        }
        else
        {
            var errors = string.Join("; ", result.Errors);
            _logger.LogError($"Quartz job '{jobName}' found errors but will complete gracefully: {errors}");
        }
    }
}