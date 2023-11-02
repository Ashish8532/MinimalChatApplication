using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalChatApplication.API.Hubs;
using MinimalChatApplication.API.Middleware;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Data.Repository;
using MinimalChatApplication.Data.Services;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Adding AutoMapper with profiles defined in the project.
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// Define and configure Swagger documentation settings for API.
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "MinimalChatAPI", Version = "v1" });

    // Configure Bearer token authentication for Swagger.
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please Enter a valid Token!",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

// Configuration manager for accessing application settings.
Microsoft.Extensions.Configuration.ConfigurationManager Configuration = builder.Configuration;

// Database connection string configuration
var connectionStrings = builder.Configuration.GetConnectionString("ChatApplicationEntities");
builder.Services.AddDbContextPool<ChatApplicationDbContext>(options => options.UseSqlServer(
connectionStrings, b => b.MigrationsAssembly("MinimalChatApplication.Data")));

// Register repositories and services in the dependency injection container.
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IGifService, GifService>();


// Configure Identity with specified options.
builder.Services.AddIdentity<ChatApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<ChatApplicationDbContext>().AddDefaultTokenProviders();


// Configures authentication services with JWT Bearer authentication.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configure JWT Bearer authentication options.
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        ValidAudience = Configuration["JWT:ValidAudience"],
        ValidIssuer = Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),
        
    };
})
.AddGoogle(options =>
{
    // Configure Google authentication options.
    options.ClientId = Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
});

// Configure Cross-Origin Resource Sharing (CORS) policy.
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", builder => builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// Add SignalR for real-time communication.
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Enable CORS with the configured policy.
app.UseCors("MyPolicy");

app.UseAuthentication();

app.UseAuthorization();

// Use custom middleware for logging HTTP requests.
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

// Map SignalR hub for real-time communication.
app.MapHub<ChatHub>("/chatHub");

app.Run();
