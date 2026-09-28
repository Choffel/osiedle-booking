using System.ComponentModel.DataAnnotations;
using Notifications.Domain.Enums;

namespace Notifications.Domain.ValueObjects;

public record NotificationRecipient
{
    public string Value { get; }
    public RecipientType Type { get; }

    public NotificationRecipient(string value, RecipientType type)
    {
        Value = value;
        Type = type;
    }

    public static NotificationRecipient FromEmail(string email)
    {
        if(string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        
        var trimmedEmail = email.Trim().ToLowerInvariant();
        
        if(!IsValidEmail(trimmedEmail))
            throw new ArgumentException("Invalid email format.", nameof(email));
        
        return new NotificationRecipient(trimmedEmail, RecipientType.Email);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\+[1-9]\d{7,14}$");
    }
    
    public override string ToString() => Value;
};