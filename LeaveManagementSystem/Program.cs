using LeaveManagementSystem.Data;
using LeaveManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - SQLServer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<LeaveRequestService>();
builder.Services.AddScoped<LeaveBalanceService>();
builder.Services.AddScoped<LeaveTypeService>();
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();