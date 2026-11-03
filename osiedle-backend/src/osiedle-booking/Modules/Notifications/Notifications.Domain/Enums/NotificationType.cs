namespace Notifications.Domain.Enums;

/// <summary>
/// Бизнес-повод отправки (подтверждение, напоминание, отмена) — нужен для выбора шаблона текста.
/// </summary>
public enum NotificationType
{
    BookingConfirmation = 1,   
    BookingReminder = 2,       
    BookingCancelled = 3,      
    PasswordReset = 4
}