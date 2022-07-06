using AccountsService.Constants.Auth;
using AccountsService.Infrastructure.Context;
using AccountsService.Models;
using AccountsService.Services;
using AccountsService.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Database
builder.Services.AddDbContext<AccountsServiceContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

//Services
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<IAccountsSvc, AccountsSvc>();

//Auth section
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AccountsServiceContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccountsPolicies.DefaultRights, policy =>
        policy.RequireRole(AccountsRoles.DefaultUser, AccountsRoles.Administrator));

    options.AddPolicy(AccountsPolicies.ElevatedRights, policy =>
        policy.RequireRole(AccountsRoles.Administrator));
});
// End of auth section

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

app.UseAuthorization();

app.MapControllers();

app.Run();
