using LogsViewerService.Infrastructure;
using LogsViewerService.Exceptions;
using LogsViewerService.Services;
using LogsViewerService.Utilities;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Services
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<ILogsViewerSvc, LogsViewerSvc>();
builder.Services.Configure<LogsContextConfiguration>(builder.Configuration.GetSection("MongoDbContext").GetSection("LogsCollection"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<GlobalExceptionsHandler>();

app.MapControllers();

Log.Information("LogsViewer service successfully started");

app.Run();
