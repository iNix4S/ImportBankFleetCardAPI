using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ImportBankFleetCardAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// อ่านค่า UseOracle จาก appsettings.json
bool useOracle = builder.Configuration.GetValue<bool>("UseOracle");

if (useOracle)
{
    builder.Services.AddScoped<IDatabaseConnectionService, OracleConnectionService>();
}
else
{
    builder.Services.AddScoped<IDatabaseConnectionService, SqlServerConnectionService>();
}

// Add services to the container.
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
