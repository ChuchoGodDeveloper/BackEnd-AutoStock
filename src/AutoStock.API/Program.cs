using System.Text;
using AutoStock.Application.Interfaces;
using AutoStock.Infrastructure;
using AutoStock.Infrastructure.Persistence;
using AutoStock.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoStock API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT",
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", null),
            new List<string>()
        },
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey no configurada");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "AutoStock",
            ValidAudience = jwtSettings["Audience"] ?? "AutoStock",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
var corsOrigins = new List<string>(allowedOrigins)
{
    "https://icy-pebble-0a56fdd00.6.azurestaticapps.net",
    "http://localhost:5173",
}.Distinct().ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AutoStockDbContext>();
    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher>();

    try
    {
        await dbContext.Database.MigrateAsync();
        await DbInitializer.SeedAsync(dbContext, passwordHasher);
        logger.LogInformation("Base de datos migrada y sembrida correctamente");
    }
    catch (Exception ex)
    {
        logger.LogWarning(
            ex,
            "No se pudo conectar a la base de datos para aplicar migraciones/seeder. Se omitirá en este arranque.");
    }
}

app.Run();
