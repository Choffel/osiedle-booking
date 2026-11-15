using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Interfaces;
using Notifications.Application.Services;
using Notifications.Infrastructure.Factories;
using Notifications.Infrastructure.Options;
using Notifications.Infrastructure.Services;
using Notifications.Infrastructure.Strategies;
using Notifications.Infrastructure.Templates;
using osiedle_booking.BuildingBlocks.Interfaces;

namespace Notifications.Infrastructure;

public static  class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
       
        var smtpOptions = configuration.GetSection("SmtpSettings").Get<SmtpOptions>()
                          ?? throw new InvalidOperationException("SmtpSettings is missing in configuration.");
        services.AddSingleton(smtpOptions);

       
        services.AddScoped<ITemplateRender, ScribanTemplateRenderer>();
        services.AddScoped<IMimeMessageFactory, MimeMessageFactory>();
        services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();

        
        services.AddScoped<INotificationChannelStrategy, EmailNotificationChannelStrategy>();
        
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<INotificationModule, NotificationService>();
        services.AddScoped<INotificationInternalApi, NotificationInternalApi>();

        return services;
    }
}