using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Business.Infrastructure;
using Business.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.WebApi.Errors;
using osiedle_booking.WebApi.Json;
using Users.Infrastructure;
using Users.Infrastructure.Data;
using Users.Infrastructure.Data.InitialData;
using Users.Infrastructure.Identity;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("Database") 
                       ?? builder.Configuration["CONNECTION_STRING"] 
                       ?? "Host=localhost;Port=5432;Database=osiedle_booking;Username=postgres;Password=postgres";

#region DI
builder.Services.AddBuildingBlocks();
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddBusinessInfrastructure(connectionString);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
#endregion

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new GuidValueObjectJsonConverter<BusinessId>(BusinessId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(
            new GuidValueObjectJsonConverter<EmployeeId>(EmployeeId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(
            new GuidValueObjectJsonConverter<BookableServiceId>(BookableServiceId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(
            new GuidValueObjectJsonConverter<WorkingHoursId>(WorkingHoursId.From, id => id.Value));
    });
builder.Services.AddEndpointsApiExplorer();

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "osiedle-booking API",
        Version = "v1"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    
    options.AddSecurityDefinition("RefreshToken", new OpenApiSecurityScheme
    {
        Name = "X-Refresh-Token",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter your Refresh Token for the X-Refresh-Token header"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "RefreshToken"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Users")));

builder.Services.AddDbContext<BusinessDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Business")));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "osiedle-booking API V1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseExceptionHandler();
app.UseCors(); // TODO: Configure CORS policy as needed
app.UseAuthentication();
app.UseAuthorization();

// using (var scope = app.Services.CreateScope()) // TODO: refactor this to a proper migration or seeding mechanism
// {
//     var services = scope.ServiceProvider;
//     var roleManager = services.GetRequiredService<RoleManager<RoleIdentity>>();
//
//     var dbContext = services.GetRequiredService<ApplicationDbContext>();
//     await dbContext.Database.MigrateAsync();
//
//     var businessDbContext = services.GetRequiredService<BusinessDbContext>();
//     await businessDbContext.Database.MigrateAsync();
//
//     await RoleInitData.InitializeAsync(roleManager);
// }

app.MapControllers();

app.Run();