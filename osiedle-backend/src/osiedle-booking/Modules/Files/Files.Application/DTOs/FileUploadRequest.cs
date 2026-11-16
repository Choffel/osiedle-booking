namespace Files.Application.DTOs;

public record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);
