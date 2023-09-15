using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Data.Services;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connection string configuration
var connectionStrings = builder.Configuration.GetConnectionString("ChatApplicationEntities");
builder.Services.AddDbContextPool<ChatApplicationDbContext>(options => options.UseSqlServer(
connectionStrings, b => b.MigrationsAssembly("MinimalChatApplication.Data")));


builder.Services.AddTransient<IUserService, UserService>();

// Configure Identity
builder.Services.AddIdentity<ChatApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<ChatApplicationDbContext>().AddDefaultTokenProviders();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
