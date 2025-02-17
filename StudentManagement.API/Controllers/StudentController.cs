using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.DTOs;
using StudentManagement.Application.Interfaces;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get all students.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStudents(
         [FromQuery] string search = "",
         [FromQuery] string sortBy = "id",
         [FromQuery] string sortDirection = "asc",
         [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10)
          {
            var result = await _studentService.GetStudentsAsync(search, sortBy, sortDirection, page, pageSize);
            return Ok(result);
           }


        /// <summary>
        /// Get student by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound(new { message = "Student not found" });

            return Ok(student);
        }

        /// <summary>
        /// Add a new student.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] StudentDto studentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var student = await _studentService.AddStudentAsync(studentDto);
            return CreatedAtAction(nameof(GetStudentById), new { id = student }, student);
        }

        /// <summary>
        /// Update student details.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentDto studentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _studentService.UpdateStudentAsync(id, studentDto);
            if (!updated)
                return NotFound(new { message = "Student not found" });

            return NoContent();
        }

        /// <summary>
        /// Soft delete a student (Deactivate account)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateStudent(int id)
        {
            var deactivated = await _studentService.DeactivateStudentAsync(id);
            if (!deactivated)
                return NotFound(new { message = "Student not found" });

            return Ok(new { message = "Student account deactivated successfully" });
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BulkUpload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV or Excel file.");

            try
            {
                var result = await _studentService.BulkUploadStudentsAsync(file);
                if (!result)
                    return BadRequest("No students were added.");

                return Ok(new { message = "Students uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/applications")]
        public async Task<IActionResult> GetStudentApplications(int id)
        {
            var applications = await _studentService.GetStudentApplicationsAsync(id);
            if (applications == null || !applications.Any())
                return NotFound(new { message = "No applications found for this student." });

            return Ok(applications);
        }

        [HttpGet("report/download")]
        public async Task<IActionResult> DownloadStudentReport()
        {
            var fileData = await _studentService.GenerateStudentReportAsync();
            if (fileData == null)
                return NotFound(new { message = "No student records available for download." });

            return File(fileData, "text/csv", "StudentReport.csv");
        }

        [HttpPost("{id}/applications")]
        public async Task<IActionResult> AddStudentApplication(int id, [FromBody] StudentApplicationDto applicationDto)
        {
            var result = await _studentService.AddStudentApplicationAsync(id, applicationDto);
            if (!result)
                return NotFound(new { message = "Student not found" });

            return Ok(new { message = "Application added successfully!" });
        }


    }
}
