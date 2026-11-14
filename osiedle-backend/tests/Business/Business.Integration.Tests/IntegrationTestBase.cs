using Business.Application.Interfaces.Category.Repositories;
using Business.Application.Interfaces.Category.Services;
using Business.Application.Services.Category;
using Business.Infrastructure.Persistence;
using Business.Infrastructure.Repositories.Category;
using Business.Infrastructure.UnitOtWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Business.Application.Interfaces;

namespace Business.Integration.Tests;

/// <summary>
/// Real CategoryService + EF repositories against an isolated in-memory SQLite
/// database (one per test), mirroring the BookingsModule integration setup.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ServiceProvider Provider;

    protected IntegrationTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BusinessDbContext>(options => options.UseSqlite(_connection));
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<ICategoryWriteRepository, CategoryWriteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryService, CategoryService>();

        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<BusinessDbContext>().Database.EnsureCreated();
    }

    protected IServiceScope CreateScope() => Provider.CreateScope();

    protected static ICategoryService Service(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ICategoryService>();

    protected static BusinessDbContext Db(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<BusinessDbContext>();

    public void Dispose()
    {
        Provider.Dispose();
        _connection.Dispose();
    }
}
