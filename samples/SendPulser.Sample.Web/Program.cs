using SendPulser;
using SendPulser.Webhooks;

var builder = WebApplication.CreateBuilder(args);

// Configuration lives under the "SendPulser" section; in development put it in user secrets.
// See appsettings.example.json for the keys.
builder.Services
    .AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName))
    .AddSendPulserResilience();

var app = builder.Build();

app.MapGet("/mailing-lists", async (ISendPulserClient sendPulse, CancellationToken cancellationToken) =>
    await sendPulse.AddressBooks.GetAllAsync(limit: 20, cancellationToken: cancellationToken));

// SendPulse signs nothing, so the secret in the path is the authentication.
var webhookSecret = builder.Configuration["SendPulser:WebhookSecret"] ?? "change-me";

app.MapSendPulserEmailWebhook(
    "/hooks/sendpulse/email/{secret}",
    (events, context, _) =>
    {
        // The HttpContext overload gives access to scoped services through context.RequestServices.
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        foreach (var received in events)
        {
            switch (received)
            {
                case EmailHardBounceEvent bounce:
                    Log.HardBounce(logger, bounce.Email, bounce.Response);
                    break;
                case UnknownEmailEvent unknown:
                    Log.UnknownEvent(logger, unknown.Event, unknown.Raw);
                    break;
                default:
                    Log.EmailEvent(logger, received.Event, received.Email, received.Timestamp);
                    break;
            }
        }

        return Task.CompletedTask;
    },
    options => options.Secret = webhookSecret);

app.MapSendPulserSmtpWebhook(
    "/hooks/sendpulse/smtp/{secret}",
    (events, _) =>
    {
        foreach (var received in events.OfType<SmtpDeliveryResultEvent>())
        {
            Log.SmtpDelivery(app.Logger, received.MessageId, received.Recipient, received.Event, received.ResponseCode);
        }

        return Task.CompletedTask;
    },
    options => options.Secret = webhookSecret);

await app.RunAsync();

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Hard bounce for {Email}: {Response}")]
    public static partial void HardBounce(ILogger logger, string? email, string? response);

    [LoggerMessage(Level = LogLevel.Information, Message = "Unknown event {EventName}: {Raw}")]
    public static partial void UnknownEvent(ILogger logger, string eventName, System.Text.Json.JsonElement raw);

    [LoggerMessage(Level = LogLevel.Information, Message = "{EventName} for {Email} at {Timestamp}")]
    public static partial void EmailEvent(ILogger logger, string eventName, string? email, DateTimeOffset? timestamp);

    [LoggerMessage(Level = LogLevel.Information, Message = "Message {MessageId} to {Recipient}: {EventName} {Code}")]
    public static partial void SmtpDelivery(ILogger logger, string? messageId, string? recipient, string eventName, int? code);
}
