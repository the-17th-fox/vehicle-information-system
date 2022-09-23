using AccountsService.Exceptions;
using AccountsService.Infrastructure.Context;
using AccountsService.Services;
using AccountsService.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication.Google;
using Common.Models.AccountsService;
using Common.Constants.Auth;
using Common.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

//Database
builder.Services.AddDbContext<AccountsServiceContext>(opt =>
    opt.UseSqlServer(config.GetConnectionString("DatabaseConnection")));

//Services
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<IAccountsSvc, AccountsSvc>();

//Auth section
builder.Services.Configure<JwtConfigurationModel>(config.GetSection("Authentication").GetSection("Jwt"));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AccountsServiceContext>();

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

    options.AddPolicy(AccountsPolicies.ElevatedRights, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AccountsRoles.Administrator);
    });
});
//End of auth section

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "VehicleInformationSystem", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionsHandler>();

app.MapControllers();

Log.Information("App successfully started");

app.Run();
