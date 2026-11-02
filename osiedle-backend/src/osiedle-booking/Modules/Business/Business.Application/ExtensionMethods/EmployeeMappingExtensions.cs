 using Business.Application.DTOs.EmployeeDTOs;
 using Business.Domain.Aggregates;

 namespace Business.Application.ExtensionMethods;

 public static class EmployeeMappingExtensions
 {
     public static EmployeeResponseDto ToResponseDto(this Employee employee)
     {
         return new EmployeeResponseDto(
             employee.Id,
             employee.BusinessId,
             employee.EmployeeName.FirstName,
             employee.EmployeeName.LastName,
             employee.Position,
             employee.Review,
             employee.Email
         );
     }
 }
