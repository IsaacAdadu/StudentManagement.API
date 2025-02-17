

using Microsoft.AspNetCore.Http;
using StudentManagement.Application.DTOs;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Application.Interfaces
{
    public interface IStudentService
    {
        Task<StudentDto> AddStudentAsync(StudentDto student);
        Task<PaginatedResult<StudentDto>> GetStudentsAsync(string search, string sortBy, string sortDirection, int page, int pageSize);
        Task<StudentDto> GetStudentByIdAsync(int id);
        Task<bool> UpdateStudentAsync(int id, StudentDto student);
        Task<bool> DeactivateStudentAsync(int id); // ✅ Rename to reflect soft delete
        Task<bool> BulkUploadStudentsAsync(IFormFile file);
        Task<IEnumerable<StudentApplication>> GetStudentApplicationsAsync(int studentId);
        Task<byte[]> GenerateStudentReportAsync();
        Task<bool> AddStudentApplicationAsync(int studentId, StudentApplicationDto applicationDto);
        
    }
}
