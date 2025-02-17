

namespace StudentManagement.Domain.Entities
{
    public class StudentApplication
    {
        public int Id { get; set; }
        public int StudentId { get; set; }  // Foreign Key
        public string ApplicationName { get; set; }
        public DateTime SubmissionDate { get; set; }
        public Student Student { get; set; }
    }
}
