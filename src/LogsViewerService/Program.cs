using LogsViewerService.Exceptions;
using LogsViewerService.Services;
using LogsViewerService.Utilities;
using Serilog;
using Common.Constants.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Common.Extensions;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

//Services
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<ILogsViewerSvc, LogsViewerSvc>();
builder.Services.Configure<LogsContextConfiguration>(config.GetSection("MongoDbContext").GetSection("LogsCollection"));

// AUTH SECTION BELOW
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt => opt.SetPredefined(
        issuer: config["Authentication:Jwt:Issuer"],
        audience: config["Authentication:Jwt:Audience"],
        signingKey: config["Authentication:Jwt:Key"]))

    .AddGoogle(GoogleDefaults.AuthenticationScheme, opt => opt.SetPredefined(
        clientId: config["Authentication:Google:ClientId"],
        clientSecret: config["Authentication:Google:ClientSecret"]));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccountsPolicies.ElevatedRights, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AccountsRoles.Administrator);
    });
});
// AUTH SECTION ABOVE

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

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionsHandler>();

app.MapControllers();

Log.Information("LogsViewer service successfully started");

app.Run();
