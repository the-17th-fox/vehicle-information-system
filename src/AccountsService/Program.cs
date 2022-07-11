using AccountsService.Constants.Auth;
using AccountsService.Exceptions;
using AccountsService.Infrastructure.Context;
using AccountsService.Models;
using AccountsService.Services;
using AccountsService.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Database
builder.Services.AddDbContext<AccountsServiceContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

//Services
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<IAccountsSvc, AccountsSvc>();

//Auth section
builder.Services.Configure<JwtConfigugartionModel>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AccountsServiceContext>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccountsPolicies.DefaultRights, policy =>
        policy.RequireRole(AccountsRoles.DefaultUser, AccountsRoles.Administrator));

    options.AddPolicy(AccountsPolicies.ElevatedRights, policy =>
        policy.RequireRole(AccountsRoles.Administrator));
});
//End of auth section

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionsHandler>();

app.MapControllers();

app.Run();
