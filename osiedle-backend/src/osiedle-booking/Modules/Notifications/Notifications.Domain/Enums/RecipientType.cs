namespace Notifications.Domain.Enums;

/// <summary>
/// Тип формата контакта (Email, телефон, Telegram ID) — нужен для валидации строки получателя.
/// </summary>
public enum RecipientType
{
    Email = 1,
    Phone = 2,
    TelegramChatId = 3
}