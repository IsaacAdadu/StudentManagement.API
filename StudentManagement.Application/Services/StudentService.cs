using CsvHelper.Configuration;
using CsvHelper;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;
using StudentManagement.Domain.Entities;
using StudentManagement.Infrastructure.Persistence;
using System.Globalization;
using System.Text;

namespace StudentManagement.Application.Services
{
    public class StudentService:IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentDto> AddStudentAsync(StudentDto studentDto)
        {
            var student = new Student
            {
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                Email = studentDto.Email,
                DateOfBirth = studentDto.DateOfBirth,
                EnrollmentDate = studentDto.EnrollmentDate
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return studentDto;
        }

        public async Task<PaginatedResult<StudentDto>> GetStudentsAsync(string search, string sortBy, string sortDirection, int page, int pageSize)
        {
            var query = _context.Students.Where(s => !s.IsDeleted);

            //  Apply Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.FirstName.Contains(search) ||
                                         s.LastName.Contains(search) ||
                                         s.Email.Contains(search));
            }

            // Apply Sorting
            switch (sortBy?.ToLower())
            {
                case "firstname":
                    query = sortDirection == "desc" ? query.OrderByDescending(s => s.FirstName) : query.OrderBy(s => s.FirstName);
                    break;
                case "lastname":
                    query = sortDirection == "desc" ? query.OrderByDescending(s => s.LastName) : query.OrderBy(s => s.LastName);
                    break;
                case "enrollmentdate":
                    query = sortDirection == "desc" ? query.OrderByDescending(s => s.EnrollmentDate) : query.OrderBy(s => s.EnrollmentDate);
                    break;
                default:
                    query = query.OrderBy(s => s.Id); 
                    break;
            }

            // Pagination
            int totalRecords = await query.CountAsync();
            var students = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    DateOfBirth = s.DateOfBirth,
                    EnrollmentDate = s.EnrollmentDate
                })
                .ToListAsync();

            return new PaginatedResult<StudentDto>
            {
                Data = students,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return null;

            return new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate
            };
        }

        public async Task<bool> UpdateStudentAsync(int id, StudentDto studentDto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;

            student.FirstName = studentDto.FirstName;
            student.LastName = studentDto.LastName;
            student.Email = studentDto.Email;
            student.DateOfBirth = studentDto.DateOfBirth;
            student.EnrollmentDate = studentDto.EnrollmentDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null || student.IsDeleted)  
            {
                return false;
            }

            student.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> BulkUploadStudentsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file uploaded");

            var students = new List<Student>();

            using (var stream = file.OpenReadStream())
            {
                if (file.FileName.EndsWith(".csv"))
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    var records = csv.GetRecords<StudentDto>().ToList();

                    students = records.Select(s => new Student
                    {
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        Email = s.Email,
                        DateOfBirth = s.DateOfBirth,
                        EnrollmentDate = s.EnrollmentDate
                    }).ToList();
                }
                else if (file.FileName.EndsWith(".xlsx"))
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    var result = reader.AsDataSet();

                    var dataTable = result.Tables[0];
                    for (int i = 1; i < dataTable.Rows.Count; i++) 
                    {
                        var row = dataTable.Rows[i];
                        students.Add(new Student
                        {
                            FirstName = row[0].ToString(),
                            LastName = row[1].ToString(),
                            Email = row[2].ToString(),
                            DateOfBirth = DateTime.Parse(row[3].ToString()),
                            EnrollmentDate = DateTime.Parse(row[4].ToString())
                        });
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid file format. Only CSV or Excel files are supported.");
                }
            }

            if (!students.Any()) return false;

            _context.Students.AddRange(students);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<StudentApplication>> GetStudentApplicationsAsync(int studentId)
        {
            return await _context.Applications
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<byte[]> GenerateStudentReportAsync()
        {
            var students = await _context.Students
                .Where(s => !s.IsDeleted) 
                .Select(s => new
                {
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.DateOfBirth,
                    s.EnrollmentDate
                })
                .ToListAsync();

            if (!students.Any())
                return null;

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(students);
            writer.Flush();

            return memoryStream.ToArray();
        }

        public async Task<bool> AddStudentApplicationAsync(int studentId, StudentApplicationDto applicationDto)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null || student.IsDeleted) return false;

            var application = new StudentApplication
            {
                StudentId = studentId,
                ApplicationName = applicationDto.ApplicationName,
                SubmissionDate = applicationDto.SubmissionDate
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return true;
        }

       

    }
}
