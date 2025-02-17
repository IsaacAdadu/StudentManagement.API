using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Services;
using StudentManagement.Domain.Entities;
using StudentManagement.Infrastructure.Persistence;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace StudentManagement.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly StudentService _studentService;
        private readonly ApplicationDbContext _dbContext;

        public StudentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
                Email = "johndoe@gmail.com",
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
                Email = "alicesmith@gmail.com",
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
                Email = "markjohnson@gmail.com",
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

        [Fact]
        public async Task GetAllStudentsAsync_Should_Return_Active_Students_Only()
        {
            // Arrange
            var student1 = new Student { FirstName = "John", LastName = "Doe", Email = "johndoe@gmail.com", IsDeleted = false };
            var student2 = new Student { FirstName = "Janet", LastName = "Doe", Email = "janetdoe@gmail.com", IsDeleted = true }; 
            _dbContext.Students.AddRange(student1, student2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentsAsync(
                search: "",
                sortBy: "id",
                sortDirection: "asc",
                page: 1,
                pageSize: 10 
            );

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1); 
            result.Data.First().Email.Should().Be("johndoe@gmail.com");
        }


        [Fact]
        public async Task UpdateStudentAsync_Should_Update_Existing_Student()
        {
            // Arrange
            var student = new Student { FirstName = "John", LastName = "Smith", Email = "Johnsmith@gmail.com" };
            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            var updateDto = new StudentDto { FirstName = "Isaac", LastName = "Smith", Email = "Isaacsmith@gmail.com" };

            // Act
            var result = await _studentService.UpdateStudentAsync(student.Id, updateDto);

            // Assert
            result.Should().BeTrue();
            var updatedStudent = await _dbContext.Students.FindAsync(student.Id);
            updatedStudent.FirstName.Should().Be("Isaac");
            updatedStudent.Email.Should().Be("Isaacsmith@gmail.com");
        }

        [Fact]
        public async Task UpdateStudentAsync_Should_Return_False_For_Nonexistent_Student()
        {
            // Arrange
            var updateDto = new StudentDto { FirstName = "New Name", LastName = "Smith", Email = "new@example.com" };

            // Act
            var result = await _studentService.UpdateStudentAsync(999, updateDto);

            // Assert
            result.Should().BeFalse();
        }
        [Fact]
        public async Task DeactivateStudentAsync_Should_Return_False_If_Already_Deleted()
        {
            // Arrange
            var student = new Student
            {
                FirstName = "Mark",
                LastName = "Johnson", 
                Email = "markjohnsen@gmail.com",
                IsDeleted = true
            };

            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.DeactivateStudentAsync(student.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddStudentApplicationAsync_Should_Fail_For_Deactivated_Student()
        {
            // Arrange
            var student = new Student
            {
                FirstName = "Amed",
                LastName = "John", 
                Email = "Amedjohn@gmail.com",
                IsDeleted = true
            };

            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            var applicationDto = new StudentApplicationDto
            {
                ApplicationName = "Internship",
                SubmissionDate = DateTime.Today
            };

            // Act
            var result = await _studentService.AddStudentApplicationAsync(student.Id, applicationDto);

            // Assert
            result.Should().BeFalse(); 
        }

        [Fact]
        public async Task GetStudentApplicationsAsync_Should_Return_Applications_If_Exist()
        {
            // Arrange
            var student = new Student
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alicesmith@gmail.com"
            };

            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            var application1 = new StudentApplication { StudentId = student.Id, ApplicationName = "Internship", SubmissionDate = DateTime.Today };
            var application2 = new StudentApplication { StudentId = student.Id, ApplicationName = "Scholarship", SubmissionDate = DateTime.Today };

            _dbContext.Applications.AddRange(application1, application2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentApplicationsAsync(student.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); 
            result.First().ApplicationName.Should().Be("Internship");
        }
        [Fact]
        public async Task GetStudentApplicationsAsync_Should_Return_Empty_List_If_No_Applications()
        {
            // Arrange
            var student = new Student
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@gmail.com"
            };

            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentApplicationsAsync(student.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); 
        }
        [Fact]
        public async Task GetStudentApplicationsAsync_Should_Return_Empty_List_If_Student_Not_Found()
        {
            
            var result = await _studentService.GetStudentApplicationsAsync(999); 
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        

    }

}

