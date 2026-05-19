using System;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();

app.MapApi();

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
            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""WarehouseInventoryProjections"" (
                    ""WarehouseId"" uuid NOT NULL,
                    ""ProductId"" uuid NOT NULL,
                    ""ProductCode"" character varying(100) NULL,
                    ""ProductName"" character varying(500) NULL,
                    ""Unit"" character varying(50) NULL,
                    ""PhysicalQuantity"" integer NOT NULL,
                    ""AllocatedQuantity"" integer NOT NULL,
                    ""WarehouseName"" character varying(200) NULL,
                    ""BrandZone"" character varying(200) NULL,
                    ""RackCode"" character varying(200) NULL,
                    ""LastSyncAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_WarehouseInventoryProjections"" PRIMARY KEY (""WarehouseId"", ""ProductId"")
                );");
            app.Logger.LogInformation("Database migration applied successfully.");
            break;
        }
        catch (Exception ex) when (attempt < maxMigrationRetries)
        {
            app.Logger.LogWarning(
                ex,
                "Database migration attempt {Attempt}/{MaxRetries} failed. Retrying in 5 seconds...",
                attempt,
                maxMigrationRetries);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database migration failed after {MaxRetries} attempts.", maxMigrationRetries);
            throw;
        }
    }
}

app.Run();