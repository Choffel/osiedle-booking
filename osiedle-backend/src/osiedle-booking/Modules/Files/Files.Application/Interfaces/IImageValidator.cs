using Files.Application.DTOs;
using Files.Domain.Enums;
using osiedle_booking.BuildingBlocks;

namespace Files.Application.Interfaces;

public interface IImageValidator
{
    Result Validate(FileUploadRequest request, FileCategory category);
}
