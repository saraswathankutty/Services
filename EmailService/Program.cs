using ACI.Auth;
using ACI.Common;
using ACI.Data;
using ACI.IServices.Main.Auth;
using ACI.IServices.Main.Dapper;
using ACI.Service;
using ACI.Service.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Token.IServices;
using Token.Services;
using Microsoft.OpenApi.Models;
using ACI.IServices.Main.Portfolio;
using ACI.Middlewares;
using ACI.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Configure JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// DbContext
var connectionString = builder.Configuration.GetSection(AppSettingConstantVariables.Connection).Value;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, ACI.Service.EmailService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IDapperService, DapperService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Background Services
builder.Services.AddHostedService<EmailLogCleanupService>();
builder.Services.AddHostedService<PeriodicLoggingService>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", (options) => {
        options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
    });

});

// Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
}

// Health Checks
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "Database");
}
else
{
    builder.Services.AddHealthChecks();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Request Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("=========================================");
app.Logger.LogInformation("EmailService API Application Started");
app.Logger.LogInformation("=========================================");

app.Run();
