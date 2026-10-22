using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs;

public record UpdateUserDto(
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces")]
    string UserName);