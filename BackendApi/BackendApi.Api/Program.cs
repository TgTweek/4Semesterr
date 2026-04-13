using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("GameDb")
    ?? throw new InvalidOperationException("Connection string 'GameDb' was not found.");

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<GameDbSeeder>();
builder.Services.AddScoped<BackendApi.Application.Interfaces.IMerchantService, BackendApi.Infrastructure.Services.MerchantService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<GameDbSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();