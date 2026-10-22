using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs;

public record RegisterUserRequest(
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "First name cannot contain spaces")]
    string FirstName,
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Last name cannot contain spaces")]
    string LastName,
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
    string Email,
    string Password
);