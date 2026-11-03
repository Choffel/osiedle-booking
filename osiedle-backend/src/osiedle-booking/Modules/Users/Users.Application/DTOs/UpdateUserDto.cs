using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs;

public record UpdateUserDto(
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "First name cannot contain spaces")]
    string FirstName,
    
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Last name cannot contain spaces")]
    string LastName
    );