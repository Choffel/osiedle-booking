using Business.Application.Interfaces;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Business;
using Business.Application.Interfaces.Employee;
using Business.Application.Services;
using Business.Application.Services.BookableItemServices;
using Business.Application.Services.BusinessServices;
using Business.Application.Services.EmployeeServices;
using Business.Infrastructure.Persistence;
using Business.Infrastructure.Repositories;
using Business.Infrastructure.Repositories.Employee;
using Business.Infrastructure.UnitOtWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Infrastructure;

//TODO: Add another services and repos to DI and rigister all in general Host  
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
        services.AddScoped<IBusinessInternalApi, BusinessInternalApi>();
        #endregion

        #region Repositories
        services.AddScoped<IBookableServiceRepository, BookableServiceRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeWorkingHoursRepository, EmployeeWorkingHoursRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        #endregion
        
        #region Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        #endregion
        
        #region Services
        services.AddScoped<IBookableServiceManagementService, BookableServiceManagementService>();
        services.AddScoped<IEmployeeManagementService, EmployeeService>();
        services.AddScoped<IEmployeeWorkingHoursService, EmployeeWorkingHoursService>();
        services.AddScoped<IBusinessManagementService, BusinessService>();

        return services;
        
        #endregion
    }
}
