using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Services;
using StudentManagement.Domain.Entities;
using StudentManagement.Infrastructure.Persistence;
using Xunit;

namespace StudentManagement.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly StudentService _studentService;
        private readonly ApplicationDbContext _dbContext;

        public StudentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _studentService = new StudentService(_dbContext);
        }

        [Fact]
        public async Task AddStudentAsync_Should_Add_Student()
        {
            // Arrange
            var studentDto = new StudentDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                EnrollmentDate = DateTime.Today
            };

            // Act
            var result = await _studentService.AddStudentAsync(studentDto);

            // Assert
            result.Should().NotBeNull();
            var studentInDb = await _dbContext.Students.FirstOrDefaultAsync(s => s.Email == studentDto.Email);
            studentInDb.Should().NotBeNull();
            studentInDb.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetStudentByIdAsync_Should_Return_Student_If_Found()
        {
            // Arrange
            var student = new Student
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                DateOfBirth = DateTime.Parse("1995-05-10"),
                EnrollmentDate = DateTime.Today
            };
            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentByIdAsync(student.Id);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Alice");
        }

        [Fact]
        public async Task GetStudentByIdAsync_Should_Return_Null_If_Not_Found()
        {
            // Act
            var result = await _studentService.GetStudentByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeactivateStudentAsync_Should_Set_IsDeleted_To_True()
        {
            // Arrange
            var student = new Student
            {
                FirstName = "Mark",
                LastName = "Johnson",
                Email = "mark@example.com",
                DateOfBirth = DateTime.Parse("1992-07-15"),
                EnrollmentDate = DateTime.Today
            };
            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.DeactivateStudentAsync(student.Id);

            // Assert
            result.Should().BeTrue();
            var deactivatedStudent = await _dbContext.Students.FindAsync(student.Id);
            deactivatedStudent.IsDeleted.Should().BeTrue();
        }
    }

}

