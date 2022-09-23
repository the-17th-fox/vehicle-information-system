using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Common.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("docker-configuration.json", optional: true, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

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

app.UseOcelot().Wait();

app.UseAuthentication();

app.MapControllers();
Log.Information("Gateway successfully started");
app.Run();
