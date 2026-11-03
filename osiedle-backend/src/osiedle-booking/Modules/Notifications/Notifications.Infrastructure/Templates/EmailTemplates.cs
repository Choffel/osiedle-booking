using System;

namespace Notifications.Infrastructure.Templates;

public static class EmailTemplates
{
    public static string GetRegistrationTemplate(string clientName, string confirmationLink)
    {
        var content = $"""
            <h2 style="color: #1a202c; margin-top: 0;">Witaj w TwojeOsiedle, {clientName}! 👋</h2>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Cieszymy się, że do nas dołączasz! Twój profil został pomyślnie utworzony.
            </p>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Aby aktywować konto i móc szybko rezerwować wizyty u lokalnych specjalistów, kliknij poniższy przycisk:
            </p>
            <div style="text-align: center; margin: 30px 0;">
                <a href="{confirmationLink}" style="background-color: #3182ce; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;">
                    Potwierdź swój e-mail
                </a>
            </div>
            <p style="color: #718096; font-size: 14px;">
                Jeśli link nie działa, skopiuj i wklej ten adres do przeglądarki:<br>
                <a href="{confirmationLink}" style="color: #3182ce;">{confirmationLink}</a>
            </p>
            """;

        return WrapInBaseLayout("Potwierdzenie rejestracji w TwojeOsiedle", content);
    }
    
    public static string GetBookingConfirmationTemplate(
        string clientName,
        string businessName,
        string serviceName,
        DateTime appointmentTime,
        string address,
        decimal price)
    {
        var content = $"""
            <h2 style="color: #2b6cb0; margin-top: 0;">Potwierdzenie rezerwacji ✂️</h2>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Cześć <strong>{clientName}</strong>! Twoja wizyta została pomyślnie zarezerwowana.
            </p>
            <div style="background-color: #edf2f7; border-left: 4px solid #3182ce; padding: 15px; margin: 20px 0; border-radius: 4px;">
                <p style="margin: 5px 0; color: #2d3748;"><strong>Miejsce:</strong> {businessName}</p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Usługa:</strong> {serviceName}</p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Data i godzina:</strong> <span style="color: #2b6cb0; font-weight: bold;">{appointmentTime:dd.MM.yyyy} o godz. {appointmentTime:HH:mm}</span></p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Adres:</strong> {address}</p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Cena:</strong> {price} PLN</p>
            </div>
            <p style="color: #4a5568; font-size: 14px; line-height: 1.5;">
                Do zobaczenia! W razie potrzeby zmiany terminu lub odwołania wizyty, możesz to zrobić w swoim panelu klienta.
            </p>
            """;

        return WrapInBaseLayout("Potwierdzenie rezerwacji", content);
    }
    
    public static string GetBookingRescheduledTemplate(
        string clientName,
        string businessName,
        string serviceName,
        DateTime oldTime,
        DateTime newTime,
        string address)
    {
        var content = $"""
            <h2 style="color: #d69e2e; margin-top: 0;">Zmiana terminu wizyty 🗓️</h2>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Cześć <strong>{clientName}</strong>! Zmieniono termin Twojej rezerwacji w <strong>{businessName}</strong>.
            </p>
            <div style="background-color: #feebc8; border-left: 4px solid #d69e2e; padding: 15px; margin: 20px 0; border-radius: 4px;">
                <p style="margin: 5px 0; color: #744210;"><strong>Usługa:</strong> {serviceName}</p>
                <p style="margin: 5px 0; color: #975a16; text-decoration: line-through;">Poprzedni termin: {oldTime:dd.MM.yyyy HH:mm}</p>
                <p style="margin: 5px 0; color: #744210; font-size: 18px;"><strong>Nowy termin: {newTime:dd.MM.yyyy} o godz. {newTime:HH:mm}</strong></p>
                <p style="margin: 5px 0; color: #744210;"><strong>Adres:</strong> {address}</p>
            </div>
            <p style="color: #4a5568; font-size: 14px;">
                Zaktualizowany termin został już zapisany w naszym systemie.
            </p>
            """;

        return WrapInBaseLayout("Zmiana terminu rezerwacji", content);
    }
    
    public static string GetBookingCancelledTemplate(
        string clientName,
        string businessName,
        string serviceName,
        DateTime appointmentTime,
        string? cancelReason = null)
    {
        var reasonBlock = !string.IsNullOrWhiteSpace(cancelReason) 
            ? $"<p style=\"margin: 5px 0; color: #742a2a;\"><strong>Powód:</strong> {cancelReason}</p>" 
            : "";

        var content = $"""
            <h2 style="color: #e53e3e; margin-top: 0;">Anulowanie wizyty ❌</h2>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Cześć <strong>{clientName}</strong>, informujemy, że Twoja rezerwacja została anulowana.
            </p>
            <div style="background-color: #fed7d7; border-left: 4px solid #e53e3e; padding: 15px; margin: 20px 0; border-radius: 4px;">
                <p style="margin: 5px 0; color: #742a2a;"><strong>Miejsce:</strong> {businessName}</p>
                <p style="margin: 5px 0; color: #742a2a;"><strong>Usługa:</strong> {serviceName}</p>
                <p style="margin: 5px 0; color: #742a2a;"><strong>Termin:</strong> {appointmentTime:dd.MM.yyyy HH:mm}</p>
                {reasonBlock}
            </div>
            <p style="color: #4a5568; font-size: 14px;">
                Zapraszamy do rezerwacji innego dogodnego terminu na stronie TwojeOsiedle.
            </p>
            """;

        return WrapInBaseLayout("Anulowanie rezerwacji", content);
    }

    public static string GetPasswordResetTemplate(string email, string token, string newPassword)
    {
        var resetLink = $"https://twojeosiedle.pl/reset?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
        var content = $"""
            <h2 style="color: #805ad5; margin-top: 0;">Reset hasła 🔐</h2>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Otrzymaliśmy prośbę o zresetowanie hasła dla konta: <strong>{email}</strong>.
            </p>
            <p style="color: #4a5568; font-size: 16px;">
                Aby ustawić nowe hasło, kliknij przycisk poniżej. Link jest ważny przez krótki czas.
            </p>
            <div style="text-align: center; margin: 20px 0;">
                <a href="{resetLink}" style="background-color: #805ad5; color: #ffffff; padding: 12px 20px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;">Zresetuj hasło</a>
            </div>
            <p style="color: #718096; font-size: 14px;">Jeśli przycisk nie działa, skopiuj i wklej ten adres do przeglądarki:<br><a href="{resetLink}" style="color: #805ad5;">{resetLink}</a></p>
            <hr />
            <p style="color: #4a5568; font-size: 14px;">Jeżeli nie prosiłeś(-aś) o zmianę hasła, zignoruj tę wiadomość. Dla bezpieczeństwa możesz rozważyć zmianę hasła jeśli podejrzewasz nieautoryzowany dostęp.</p>
            """;

        return WrapInBaseLayout("Reset hasła — TwojeOsiedle", content);
    }

    public static string GetBookingConfirmationTemplate(
        string? clientEmail,
        string? clientPhoneNumber,
        TimeSpan serviceDuration,
        string serviceDescription,
        decimal price)
    {
        var durationText = serviceDuration.TotalMinutes >= 60
            ? $"{(int)serviceDuration.TotalHours}h {serviceDuration.Minutes}m"
            : $"{(int)serviceDuration.TotalMinutes} min";

        var content = $"""
            <h2 style="color: #2b6cb0; margin-top: 0;">Potwierdzenie rezerwacji ✨</h2>
            <p style="color: #4a5568; font-size: 16px; line-height: 1.5;">
                Dziękujemy! Otrzymaliśmy Twoją rezerwację.
            </p>
            <div style="background-color: #edf2f7; border-left: 4px solid #3182ce; padding: 15px; margin: 20px 0; border-radius: 4px;">
                <p style="margin: 5px 0; color: #2d3748;"><strong>Usługa:</strong> {serviceDescription}</p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Czas trwania:</strong> {durationText}</p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Cena:</strong> {price} PLN</p>
                <p style="margin: 5px 0; color: #2d3748;"><strong>Kontakt:</strong> {clientEmail ?? "-"} { (string.IsNullOrWhiteSpace(clientPhoneNumber) ? "" : $"| {clientPhoneNumber}") }</p>
            </div>
            <p style="color: #4a5568; font-size: 14px;">W razie potrzeby zmiany terminu lub odwołania wizyty, skorzystaj z panelu klienta lub skontaktuj się bezpośrednio z usługodawcą.</p>
            """;

        return WrapInBaseLayout("Potwierdzenie rezerwacji", content);
    }
    
    private static string WrapInBaseLayout(string title, string bodyContent)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{title}</title>
            </head>
            <body style="background-color: #f7fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; margin: 0; padding: 20px;">
                <table align="center" border="0" cellpadding="0" cellspacing="0" width="100%" style="max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color: #2b6cb0; padding: 20px; text-align: center;">
                            <h1 style="color: #ffffff; margin: 0; font-size: 24px; font-weight: bold; letter-spacing: 1px;">TwojeOsiedle</h1>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding: 30px;">
                            {bodyContent}
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color: #edf2f7; padding: 20px; text-align: center; color: #a0aec0; font-size: 12px;">
                            <p style="margin: 0;">Wiadomość wygenerowana automatycznie przez serwis <strong>TwojeOsiedle</strong>.</p>
                            <p style="margin: 5px 0 0 0;">Lokalne usługi na Twoim osiedlu.</p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}