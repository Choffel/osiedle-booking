using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs;

public record ForgotPasswordRequest(
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
    string Email
    );