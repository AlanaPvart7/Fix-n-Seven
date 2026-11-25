using DotNetEnv;
using FixnSeven.Infrastructure;

Env.Load();   // lee FixnSeven.API/.env

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión en el .env");

builder.Services.AddInfrastructure(connectionString);   // base de datos + repositorios
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();
app.Run();