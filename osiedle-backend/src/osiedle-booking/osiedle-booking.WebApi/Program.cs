using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using osiedle_booking.BuildingBlocks;
using Users.Infrastructure;
using Users.Infrastructure.Data.InitialData;
using Users.Infrastructure.Identity;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

#region DI
builder.Services.AddBuildingBlocks();
builder.Services.AddUsersInfrastructure(builder.Configuration);
#endregion

builder.Services.AddControllers();
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

app.UseCors(); // TODO: Configure CORS policy as needed
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope()) // TODO: refactor this to a proper migration or seeding mechanism
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<RoleIdentity>>();
    
    await RoleInitData.InitializeAsync(roleManager);
}

app.MapControllers();

app.Run();