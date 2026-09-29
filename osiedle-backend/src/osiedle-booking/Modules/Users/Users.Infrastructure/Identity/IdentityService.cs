using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using osiedle_booking.BuildingBlocks;
using Users.Appliation.Abstractions;
using Users.Appliation.DTOs.Identity;

namespace Users.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(UserManager<ApplicationUser> userManager, ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<Guid>> CreateIdentityAsync(string email, string password, CancellationToken ct)
    {
        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email
        };

        var result = await _userManager.CreateAsync(identityUser, password);

        return result.Succeeded
            ? osiedle_booking.BuildingBlocks.Result<Guid>.Success(identityUser.Id)
            : osiedle_booking.BuildingBlocks.Result<Guid>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
    
    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("User not found");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }
    
    public async Task<IdentityUserDto> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        var response = new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: user != null
        );
        
        return response;
    }
    
    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        return isValid;
    }
    
    public async Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("User not found");  

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }
    
    public async Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<IdentityUserDto> AddToRoleAsync(Guid userId, string roleName, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new IdentityUserDto(
                Id: Guid.Empty, 
                UserName: null,
                Email: null,
                EmailConfirmed: false,
                Succeeded: false
            );

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return new IdentityUserDto(
                Id: Guid.Empty, 
                UserName: null,
                Email: null,
                EmailConfirmed: false,
                Succeeded: false
            );

        return new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: true
        );
    }
    
    public async Task<IdentityUserDto> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new IdentityUserDto(
                Id: Guid.Empty, 
                UserName: null,
                Email: null,
                EmailConfirmed: false,
                Succeeded: false
            );

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return new IdentityUserDto(
                Id: Guid.Empty, 
                UserName: null,
                Email: null,
                EmailConfirmed: false,
                Succeeded: false
            );

        return new IdentityUserDto(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            EmailConfirmed: user.EmailConfirmed,
            Succeeded: true
        );
    }
}