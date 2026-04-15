using SensorX.Master.Infrastructure.DI;
using SensorX.Master.WebApi.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SensorX.Master.Infrastructure.Persistences;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Jwt:RequireHttpsMetadata");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false, // Thường Gateway đã validate rồi
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
                "Master API migration attempt {Attempt}/{MaxRetries} failed. Retrying in 5 seconds...",
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

