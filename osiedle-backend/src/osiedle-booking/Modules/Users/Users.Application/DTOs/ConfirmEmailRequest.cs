namespace Users.Application.DTOs;

public record ConfirmEmailRequest(string Email, string Token);