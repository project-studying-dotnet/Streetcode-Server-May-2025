using Microsoft.OpenApi.Models;

namespace UserService.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "User-Service", Version = "v1" });
            opt.CustomSchemaIds(x => x.FullName);
        });
    }
}