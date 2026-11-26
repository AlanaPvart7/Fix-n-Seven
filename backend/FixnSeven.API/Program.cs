using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FixnSeven.API.Middleware;
using FixnSeven.API.Security;
using FixnSeven.Application.Common.Interfaces;
using FixnSeven.Infrastructure;
using Scalar.AspNetCore;
using FixnSeven.Application;

Env.Load();   // lee FixnSeven.API/.env

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión en el .env");

builder.Services.AddApplication();
var storagePath = builder.Configuration["Storage:Path"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "storage");
builder.Services.AddInfrastructure(connectionString, storagePath);   // base de datos + repositorios + archivos
// JWT: valida el token que manda el frontend en cada peticion
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta Jwt__Key en el .env");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FixnSeven";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Tenant actual: lee enterprise_id del token en cada peticion
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, JwtCurrentTenant>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();   // primero: atrapa los errores de todo lo que sigue

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // genera el contrato JSON de la API
    app.MapScalarApiReference();    // interfaz visual en /scalar/v1
}

app.UseAuthentication();   // 1. quien eres (lee el token)
app.UseAuthorization();    // 2. que puedes hacer (roles)

app.MapControllers();
app.Run();