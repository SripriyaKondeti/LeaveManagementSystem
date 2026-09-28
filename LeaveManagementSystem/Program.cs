using System.Text;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Middleware;
using LeaveManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "The supplied value is invalid."
                            : error.ErrorMessage)
                        .ToArray());

            var response = ApiResponseDto<object>.Failure(
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                context.HttpContext.TraceIdentifier,
                errors);

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description =
                "Enter: Bearer {your JWT token}"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type =
                                Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// Database - SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString("DefaultConnection")
        )
);

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<LeaveRequestService>();
builder.Services.AddScoped<LeaveBalanceService>();
builder.Services.AddScoped<LeaveTypeService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT key is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponseDto<object>.Failure(
                StatusCodes.Status401Unauthorized,
                "Authentication is required or the token is invalid."));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponseDto<object>.Failure(
                StatusCodes.Status403Forbidden,
                "You do not have permission to access this resource."));
        }
    };

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),

            ValidateIssuer = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await DbSeeder.SeedUsersAsync(context);
}

// Enable wwwroot
app.UseStaticFiles();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.InjectJavascript(
            "/swagger-role-filter.js");
    });
}

app.UseHttpsRedirection();

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
