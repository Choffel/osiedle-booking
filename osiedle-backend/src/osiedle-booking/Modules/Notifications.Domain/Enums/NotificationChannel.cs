namespace Notifications.Domain.Enums;

/// <summary>
/// Канал доставки (SMTP, WhatsApp, Telegram, SMS) — определяет, какой сервис вызывать для отправки.
/// </summary>
public enum NotificationChannel
{
    Email,
    Telegram,
    WhatsApp
}