using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
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

// Configure Identity User 
builder.Services.AddIdentity<ChatApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ChatApplicationDbContext>()
    .AddDefaultTokenProviders();


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
