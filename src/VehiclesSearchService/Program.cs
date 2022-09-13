using Serilog;
using VehiclesSearchService.Exceptions;
using VehiclesSearchService.Services;
using VehiclesSearchService.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IManufacturersSearchSvc, ManufacturersSearchSvc>();
builder.Services.Configure<NhtsaApiConfig>(builder.Configuration.GetSection("NHTSA"));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<GlobalExceptionsHandler>();

app.MapControllers();

Log.Information("Vehicles Search Service successfully started");

app.Run();
