using Business.Application.Interfaces;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Categories.Services;
using Business.Application.Services.CategoryServices;
using Business.Infrastructure.Persistence;
using Business.Infrastructure.Repositories.Business;
using Business.Infrastructure.UnitOtWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Integration.Tests;

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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
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
