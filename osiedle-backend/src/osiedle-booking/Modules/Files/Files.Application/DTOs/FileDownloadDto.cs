namespace Files.Application.DTOs;

public record FileDownloadDto(
    Stream Stream,
    string ContentType,
    string FileName);
