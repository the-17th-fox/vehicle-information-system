using AccountsService.Auth;
using AccountsService.Infrastructure.Context;
using AccountsService.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

//Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AccountsServiceContext>();

//Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccountsPolicies.DefaultRights, policy =>
        policy.RequireRole(AccountsRoles.DefaultUser, AccountsRoles.Administrator));

    options.AddPolicy(AccountsPolicies.ElevatedRights, policy =>
        policy.RequireRole(AccountsRoles.Administrator));
});

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
