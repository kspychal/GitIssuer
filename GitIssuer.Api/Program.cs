using GitIssuer.Core.Configuration;
using GitIssuer.Core.Factories;
using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services;
using Microsoft.Extensions.Options;
using Serilog;

namespace GitIssuer.Api;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            Log.Information("Starting up");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IGitServiceFactory, GitServiceFactory>();
            builder.Services.AddHttpClient();

            builder.Services.Configure<GitTokensOptions>(
                builder.Configuration.GetSection("GitTokens"));

            builder.Services.AddScoped(serviceProvider =>
            {
                var tokens = serviceProvider.GetRequiredService<IOptions<GitTokensOptions>>().Value;
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

                if (string.IsNullOrWhiteSpace(tokens.GitHubToken))
                    throw new InvalidOperationException("GitHub token is missing.");

                return new GitHubService(httpClientFactory, tokens.GitHubToken);
            });

            builder.Services.AddScoped(serviceProvider =>
            {
                var tokens = serviceProvider.GetRequiredService<IOptions<GitTokensOptions>>().Value;
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

                if (string.IsNullOrWhiteSpace(tokens.GitLabToken))
                    throw new InvalidOperationException("GitLab token is missing.");

                return new GitLabService(httpClientFactory, tokens.GitHubToken);
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            Log.Information("Application started successfully and is now running");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}