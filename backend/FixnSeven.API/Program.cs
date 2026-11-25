using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using FixnSeven.Infrastructure.Data;

Env.Load();   // lee FixnSeven.API/.env

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión en el .env");

builder.Services.AddDbContext<FixnSevenDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();
app.Run();