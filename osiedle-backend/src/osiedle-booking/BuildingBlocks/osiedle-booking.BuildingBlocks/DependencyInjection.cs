using Microsoft.Extensions.DependencyInjection;
using osiedle_booking.BuildingBlocks.Interfaces;
using osiedle_booking.BuildingBlocks.Services;

namespace osiedle_booking.BuildingBlocks;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}