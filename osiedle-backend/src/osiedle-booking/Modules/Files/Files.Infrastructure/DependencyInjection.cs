using Azure.Storage.Blobs;
using Files.Application.Interfaces;
using Files.Application.Services;
using Files.Infrastructure.Generators;
using Files.Infrastructure.Options;
using Files.Infrastructure.Persistence;
using Files.Infrastructure.Repositories;
using Files.Infrastructure.Storage;
using Files.Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Files.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFilesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? connectionString = null)
    {
        services.Configure<AzureBlobStorageOptions>(
            configuration.GetSection(AzureBlobStorageOptions.SectionName));

        var azureOptions = configuration.GetSection(AzureBlobStorageOptions.SectionName)
            .Get<AzureBlobStorageOptions>();

        if (azureOptions is not null && !string.IsNullOrWhiteSpace(azureOptions.ConnectionString))
        {
            services.AddSingleton(new BlobServiceClient(azureOptions.ConnectionString));
        }

        var dbConnectionString = connectionString
            ?? configuration.GetConnectionString("Database")
            ?? configuration["CONNECTION_STRING"];

        if (!string.IsNullOrWhiteSpace(dbConnectionString))
        {
            services.AddDbContext<FilesDbContext>(options =>
                options.UseNpgsql(dbConnectionString));
        }

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IImageValidator, ImageValidator>();
        services.AddScoped<IBlobPathGenerator, BlobPathGenerator>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileModule, FileService>();

        return services;
    }
}
