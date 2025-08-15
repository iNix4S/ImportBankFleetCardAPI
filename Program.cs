using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ImportBankFleetCardAPI.Services;
using ImportBankFleetCardAPI.Repositories;
using ImportBankFleetCardAPI.Logging;
using ImportBankFleetCardAPI.Config;

var builder = WebApplication.CreateBuilder(args);

// อ่านค่า UseOracle จาก appsettings.json

// Always use OracleConnectionService for IDatabaseConnectionService (refactor for Oracle only)
builder.Services.AddScoped<IDatabaseConnectionService, OracleConnectionService>();

builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IFleetCardRepository, FleetCardRepository>();
builder.Services.AddControllers();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IFleetCardRepository, FleetCardRepository>();
builder.Services.AddScoped<ILoggingService, DbLoggingService>();
builder.Services.AddControllers();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IFleetCardRepository, FleetCardRepository>();
builder.Services.AddScoped<ILoggingService, DbLoggingService>();
builder.Services.AddScoped<IConfigService, DbConfigService>();
builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddMemoryCache();
// Register services with correct constructor dependencies
// FleetCardRepository, DbLoggingService, DbConfigService now require IDatabaseConnectionService
// (MemoryCache already registered above)
builder.Services.AddScoped<IFleetCardRepository, FleetCardRepository>();
builder.Services.AddScoped<ILoggingService, DbLoggingService>();
builder.Services.AddScoped<IConfigService, DbConfigService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
