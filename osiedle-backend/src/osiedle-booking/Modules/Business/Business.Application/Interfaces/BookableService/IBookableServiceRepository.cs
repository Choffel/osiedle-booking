using Business.Application.DTOs.BookableServiceDTOs;
using Business.Domain.ValueObject;


namespace Business.Application.Interfaces.BookableService;

public interface IBookableServiceRepository 
{
    Task <Domain.Entities.BookableService> GetByIdAsync(BookableServiceId id, CancellationToken ct);
    Task <ServiceShortInfoDto> GetBasicInfoByIdAsync(BookableServiceId id, CancellationToken ct);
    
    void Add(Domain.Entities.BookableService service, CancellationToken ct);
    void Update(Domain.Entities.BookableService service, CancellationToken ct);
    void Delete(Domain.Entities.BookableService service, CancellationToken ct);
}
