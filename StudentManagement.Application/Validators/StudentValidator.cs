

using FluentValidation;
using StudentManagement.Application.DTOs;

namespace StudentManagement.Application.Validators
{
    public class StudentValidator : AbstractValidator<StudentDto>
    {
        public StudentValidator()
        {
            RuleFor(s => s.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must be less than 50 characters.");

            RuleFor(s => s.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must be less than 50 characters.");

            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(s => s.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Today).WithMessage("Date of birth cannot be in the future.");

            RuleFor(s => s.EnrollmentDate)
                .NotEmpty().WithMessage("Enrollment date is required.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Enrollment date cannot be in the future.");
        }
    }
    
}
