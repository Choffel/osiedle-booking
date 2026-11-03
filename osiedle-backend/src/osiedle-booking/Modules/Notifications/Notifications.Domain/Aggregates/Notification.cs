using Notifications.Domain.Enums;
using Notifications.Domain.ValueObjects;

namespace Notifications.Domain.Aggregates;

public class Notification
{
    public Guid Id { get; private set; }
    public NotificationRecipient Recipient { get; private set; } = null!;
    public NotificationChannel Channel { get; private set; }
    public NotificationType Type { get; private set; }
    
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    
    public NotificationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    private Notification() { } 
    
    public static Notification Create(
        NotificationRecipient recipient,
        NotificationChannel channel,
        NotificationType type,
        string subject,
        string body)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            Recipient = recipient,
            Channel = channel,
            Type = type,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
    }
    
    public void MarkAsFailed(string error)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = error;
    }
}