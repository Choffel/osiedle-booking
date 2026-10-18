using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Employee;
using Business.Application.Services;
using Business.Infrastructure.Persistence;
using Business.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessInfrastructure(this IServiceCollection services, string connectionString)
    {
        #region Database
        services.AddDbContext<BusinessDbContext>(options =>
            options.UseNpgsql(connectionString));
        #endregion

        #region Internal API
        services.AddScoped<IBookableServiceInternalApi, BookableServiceInternalApi>();
        services.AddScoped<IEmployeeInternalApi, EmployeeInternalApi>();
        #endregion

        #region Repositories
        services.AddScoped<IBookableServiceRepository, BookableServiceRepository>();
        #endregion

        return services;
    }
}
