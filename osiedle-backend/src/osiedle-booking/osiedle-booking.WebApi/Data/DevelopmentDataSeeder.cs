using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Infrastructure.Data;
using Business.Domain.Aggregates;
using Business.Domain.Entities;
using Business.Domain.Enums;
using Business.Domain.ValueObjects;
using Business.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.Roles;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.ValueObjects;
using Users.Infrastructure.Data;
using Users.Infrastructure.Identity.Models;
using BookingDbContext = Bookings.Infrastructure.Data.ApplicationDbContext;
using DomainBusiness = Business.Domain.Aggregates.Business;
using DomainUser = Users.Domain.Entity.User.User;
using BusinessIdValue = Business.Domain.ValueObjects.BusinessId;
using EmployeeIdValue = Business.Domain.ValueObjects.EmployeeId;
using BookableServiceIdValue = Business.Domain.ValueObject.BookableServiceId;
using WorkingHoursIdValue = Business.Domain.ValueObjects.WorkingHoursId;
using BookingBusinessId = Bookings.Domain.Entities.Booking.Value_objects.BusinessId;
using BookingServiceId = Bookings.Domain.Entities.Booking.Value_objects.ServiceId;
using BookingStaffId = Bookings.Domain.Entities.Booking.Value_objects.StaffId;
using BookingClientId = Bookings.Domain.Entities.Booking.Value_objects.ClientId;

namespace osiedle_booking.WebApi.Data;

public sealed class DevelopmentDataSeeder
{
    private static readonly Guid AdminId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid OwnerId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid ClientId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid SecondClientId = Guid.Parse("10000000-0000-0000-0000-000000000004");

    private static readonly Guid BusinessGuid = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondBusinessId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid EmployeeGuid = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondEmployeeId = Guid.Parse("40000000-0000-0000-0000-000000000002");
    private static readonly Guid SecondBusinessEmployeeId = Guid.Parse("40000000-0000-0000-0000-000000000003");
    private static readonly Guid ServiceGuid = Guid.Parse("50000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondServiceId = Guid.Parse("50000000-0000-0000-0000-000000000002");
    private static readonly Guid ThirdServiceId = Guid.Parse("50000000-0000-0000-0000-000000000003");
    private static readonly Guid BookingGuid = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondBookingId = Guid.Parse("60000000-0000-0000-0000-000000000002");

    public async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<RoleIdentity> roleManager,
        Users.Infrastructure.Data.ApplicationDbContext usersDb,
        BusinessDbContext businessDb,
        BookingDbContext bookingDb,
        CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager, usersDb, cancellationToken);
        await SeedBusinessDataAsync(businessDb, cancellationToken);
        await SeedBookingsAsync(bookingDb, cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<RoleIdentity> roleManager)
    {
        foreach (var role in UserRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(new RoleIdentity(role));
            EnsureSucceeded(result, $"role '{role}'");
        }
    }

    private static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        Users.Infrastructure.Data.ApplicationDbContext usersDb,
        CancellationToken cancellationToken)
    {
        var password = "Test123!";
        var users = new[]
        {
            (Id: AdminId, Email: "admin@osiedle-booking.local", FirstName: "Anna", LastName: "Administrator", Role: UserRoles.Admin),
            (Id: OwnerId, Email: "owner@osiedle-booking.local", FirstName: "Marek", LastName: "Właściciel", Role: UserRoles.User),
            (Id: ClientId, Email: "client@osiedle-booking.local", FirstName: "Julia", LastName: "Klient", Role: UserRoles.User),
            (Id: SecondClientId, Email: "client2@osiedle-booking.local", FirstName: "Piotr", LastName: "Klient", Role: UserRoles.User)
        };

        foreach (var seedUser in users)
        {
            var identityUser = await userManager.FindByIdAsync(seedUser.Id.ToString());
            if (identityUser is null)
            {
                identityUser = new ApplicationUser
                {
                    Id = seedUser.Id,
                    Email = seedUser.Email,
                    UserName = seedUser.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(identityUser, password);
                EnsureSucceeded(createResult, $"identity user '{seedUser.Email}'");
            }

            if (!await userManager.IsInRoleAsync(identityUser, seedUser.Role))
            {
                var roleResult = await userManager.AddToRoleAsync(identityUser, seedUser.Role);
                EnsureSucceeded(roleResult, $"role assignment for '{seedUser.Email}'");
            }

            if (!await usersDb.Users.AnyAsync(user => user.Id == UserId.From(seedUser.Id), cancellationToken))
            {
                usersDb.Users.Add(DomainUser.Create(
                    UserId.From(seedUser.Id),
                    Name.Create(seedUser.FirstName, seedUser.LastName),
                    seedUser.Email));
            }
        }

        await usersDb.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedBusinessDataAsync(
        BusinessDbContext db,
        CancellationToken cancellationToken)
    {
        var businessesToSeed = new[]
        {
            DomainBusiness.Create(
                BusinessIdValue.From(BusinessGuid),
                "Pracownia Urody",
                UserId.From(OwnerId),
                "Kameralne studio stylizacji i pielęgnacji.",
                Location.Create("Warszawa", "ul. Główna 10"),
                Category.FacialCare),
            DomainBusiness.Create(
                BusinessIdValue.From(SecondBusinessId),
                "Studio Harmonia",
                UserId.From(AdminId),
                "Gabinet masażu i usług wellness.",
                Location.Create("Kraków", "ul. Parkowa 5"),
                Category.Waxing)
        };

        foreach (var business in businessesToSeed)
        {
            if (!await db.Businesses.AnyAsync(existing => existing.Id == business.Id, cancellationToken))
            {
                db.Businesses.Add(business);
            }
        }

        var employeesToSeed = new[]
        {
            Employee.Create(EmployeeIdValue.From(EmployeeGuid), BusinessIdValue.From(BusinessGuid), "Katarzyna", "Nowak",
                "Stylistka", "Doświadczona stylistka paznokci.", "kasia@pracownia.local"),
            Employee.Create(EmployeeIdValue.From(SecondEmployeeId), BusinessIdValue.From(BusinessGuid), "Tomasz", "Lis",
                "Barber", "Specjalista od klasycznych cięć.", "tomasz@pracownia.local"),
            Employee.Create(EmployeeIdValue.From(SecondBusinessEmployeeId), BusinessIdValue.From(SecondBusinessId), "Ewa", "Mazur",
                "Masażystka", "Certyfikowana terapeutka.", "ewa@harmonia.local")
        };

        foreach (var employee in employeesToSeed)
        {
            if (!await db.Employees.AnyAsync(existing => existing.Id == employee.Id, cancellationToken))
            {
                db.Employees.Add(employee);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var servicesToSeed = new[]
        {
            CreateSeedService(ServiceGuid, EmployeeGuid, BusinessGuid, Category.FacialCare,
                "Manicure hybrydowy", "Trwały manicure z opracowaniem skórek.", 120m, 60),
            CreateSeedService(SecondServiceId, SecondEmployeeId, BusinessGuid, Category.Haircut,
                "Strzyżenie męskie", "Klasyczne strzyżenie i stylizacja.", 90m, 45),
            CreateSeedService(ThirdServiceId, SecondBusinessEmployeeId, SecondBusinessId, Category.Waxing,
                "Masaż relaksacyjny", "Masaż całego ciała.", 180m, 90)
        };

        foreach (var service in servicesToSeed)
        {
            var existing = await db.BookableServices
                .FirstOrDefaultAsync(candidate => candidate.Id == service.Id, cancellationToken);
            if (existing is null)
            {
                db.BookableServices.Add(service);
            }
            else
            {
                existing.AddToBusiness(service.BusinessId!.Value);
            }
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstWorkingHoursId = WorkingHoursIdValue.From(Guid.Parse("70000000-0000-0000-0000-000000000001"));
        var workingHoursToSeed = new[]
        {
            WorkingHours.Create(firstWorkingHoursId,
                EmployeeIdValue.From(EmployeeGuid), today, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            WorkingHours.Create(WorkingHoursIdValue.From(Guid.Parse("70000000-0000-0000-0000-000000000002")),
                EmployeeIdValue.From(SecondEmployeeId), today, new TimeOnly(10, 0), new TimeOnly(18, 0)),
            WorkingHours.Create(WorkingHoursIdValue.From(Guid.Parse("70000000-0000-0000-0000-000000000003")),
                EmployeeIdValue.From(SecondBusinessEmployeeId), today, new TimeOnly(8, 0), new TimeOnly(16, 0))
        };

        foreach (var hours in workingHoursToSeed)
        {
            if (!await db.WorkingHours.AnyAsync(existing => existing.Id == hours.Id, cancellationToken))
            {
                db.WorkingHours.Add(hours);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static BookableService CreateSeedService(
        Guid serviceId,
        Guid employeeId,
        Guid businessId,
        Category category,
        string name,
        string description,
        decimal price,
        int durationMinutes)
    {
        var service = BookableService.Create(
            BookableServiceIdValue.From(serviceId),
            EmployeeIdValue.From(employeeId),
            category,
            name,
            description,
            price,
            TimeSpan.FromMinutes(durationMinutes));
        service.AddToBusiness(BusinessIdValue.From(businessId));
        return service;
    }

    private static async Task SeedBookingsAsync(BookingDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Bookings.AnyAsync(booking => booking.Id == BookingId.From(BookingGuid), cancellationToken))
            return;

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var completed = Booking.Create(
            BookingId.From(BookingGuid),
            BookingStaffId.From(EmployeeGuid),
            BookingBusinessId.From(BusinessGuid),
            BookingClientId.From(ClientId),
            BookingServiceId.From(ServiceGuid),
            TimeSlot.Create(tomorrow.AddHours(10), tomorrow.AddHours(11)),
            Money.Pln(120m));
        completed.Complete();

        db.Bookings.AddRange(
            completed,
            Booking.Create(
                BookingId.From(SecondBookingId),
                BookingStaffId.From(SecondEmployeeId),
                BookingBusinessId.From(BusinessGuid),
                BookingClientId.From(SecondClientId),
                BookingServiceId.From(SecondServiceId),
                TimeSlot.Create(tomorrow.AddHours(12), tomorrow.AddHours(12.75)),
                Money.Pln(90m)));

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Development data seed failed during {operation}: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }
    }
}
