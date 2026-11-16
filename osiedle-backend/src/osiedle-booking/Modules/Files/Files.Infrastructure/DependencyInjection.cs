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
        string connectionString)
    {
        services.Configure<AzureBlobStorageOptions>(
            configuration.GetSection(AzureBlobStorageOptions.SectionName));

        var azureOptions = configuration.GetSection(AzureBlobStorageOptions.SectionName)
            .Get<AzureBlobStorageOptions>();

        var blobConnectionString = azureOptions?.ConnectionString;
        if (!string.IsNullOrWhiteSpace(blobConnectionString))
        {
            services.AddSingleton(new BlobServiceClient(blobConnectionString));
        }
        else if (!string.IsNullOrWhiteSpace(azureOptions?.ServiceUri))
        {
            services.AddSingleton(new BlobServiceClient(new Uri(azureOptions.ServiceUri)));
        }
        else
        {
            services.AddSingleton(new BlobServiceClient("UseDevelopmentStorage=true"));
        }

        services.AddDbContext<FilesDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Files")));

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
