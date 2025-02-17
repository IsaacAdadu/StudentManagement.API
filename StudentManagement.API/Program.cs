using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StudentManagement.API.Filters;
using StudentManagement.API.Middleware;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;
using StudentManagement.Application.Services;
using StudentManagement.Application.Validators;
using StudentManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("StudentManagementConnectionString"),
        b => b.MigrationsAssembly("StudentManagement.Infrastructure") 
    ));
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<StudentDto>, StudentValidator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Register Swagger with File Upload Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student Management API", Version = "v1" });

    // Register file upload operation filter
    c.OperationFilter<FileUploadOperationFilter>();
});

var app = builder.Build();


// register global exception middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
