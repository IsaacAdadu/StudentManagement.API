

using static System.Net.Mime.MediaTypeNames;

namespace StudentManagement.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<StudentApplication> Applications { get; set; } = new();
    }
}
