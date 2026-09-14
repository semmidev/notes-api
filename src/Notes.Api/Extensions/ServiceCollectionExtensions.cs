using Notes.Api.Authentication;
using Notes.Application.Notes;

namespace Notes.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application service bersifat stateless, jadi scoped cukup untuk request lifecycle.
        services.AddScoped<NoteService>();
        return services;
    }

    public static IServiceCollection AddBasicAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(BasicAuthOptions.SectionName);
        services.Configure<BasicAuthOptions>(section);

        var username = section["Username"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("BasicAuth:Username dan BasicAuth:Password wajib dikonfigurasi.");

        services
            .AddAuthentication("Basic")
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });

        services.AddAuthorization();
        return services;
    }
}
