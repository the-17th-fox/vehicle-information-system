using Common.Constants.Auth;
using Common.Extensions;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using StackExchange.Redis;
using VehiclesSearchService.Exceptions;
using VehiclesSearchService.Infrastructure;
using VehiclesSearchService.Services;
using VehiclesSearchService.Utilities;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddScoped<IManufacturersSearchSvc, ManufacturersSearchSvc>();
builder.Services.Configure<NhtsaApiConfig>(builder.Configuration.GetSection("NHTSA"));

builder.Services.Configure<RedisCacheConfig>(config.GetSection("DistributedCache").GetSection("Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config["DistributedCache:Redis:ConnectionString"]));
builder.Services.AddScoped<ICacheRepository, CacheRepository>();

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
    options.AddPolicy(AccountsPolicies.DefaultRights, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AccountsRoles.DefaultUser, AccountsRoles.Administrator);
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionsHandler>();

app.MapControllers();

Log.Information("Vehicles Search Service successfully started");

app.Run();
