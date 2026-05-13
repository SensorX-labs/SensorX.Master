using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Infrastructure.DI;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Master.WebApi.API;
using SensorX.Master.WebApi.Configurations;
using SensorX.Master.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);
// Cấu hình Authentication & Authorization (Tin tưởng Gateway)
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();

builder.Services.AddServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Yêu cầu .NET tự động chuyển đổi giữa String và Enum
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
    options.CustomSchemaIds(x => x.FullName);
});

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

var autoApplyMigration = builder.Configuration.GetValue("Migration:AutoApply", true);
if (autoApplyMigration)
{
    const int maxMigrationRetries = 12;
    for (var attempt = 1; attempt <= maxMigrationRetries; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();

            break;
        }
        catch (Exception ex) when (attempt < maxMigrationRetries)
        {
            app.Logger.LogWarning(
                ex,
                "Data API migration attempt {Attempt}/{MaxRetries} failed. Retrying in 5 seconds...",
                attempt,
                maxMigrationRetries);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(opt => { });

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();

app.MapApi();

app.Run();
