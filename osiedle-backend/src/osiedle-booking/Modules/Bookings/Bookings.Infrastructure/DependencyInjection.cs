using Bookings.Application.Abstractions.Repositories;
using Bookings.Application.Abstractions.Services;
using Bookings.Application.Services;
using Bookings.Domain.Services;
using Bookings.Infrastructure.Data;
using Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database
        var connectionString = configuration["CONNECTION_STRING"];
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        #endregion

        #region Services
        services.AddScoped<IBookingService, BookingService>();
        #endregion
        
        services.AddScoped<ISlotCalculator, SlotCalculator>();
        
        #region Repositories
        services.AddScoped<IBookingRepository, BookingRepository>();
        #endregion
        
        return services;
    }
}