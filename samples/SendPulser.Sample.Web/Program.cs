using SendPulser;
using SendPulser.Webhooks;

var builder = WebApplication.CreateBuilder(args);

// Configuration lives under the "SendPulser" section; in development put it in user secrets.
builder.Services
    .AddSendPulser(builder.Configuration.GetSection(SendPulserOptions.SectionName))
    .AddSendPulserResilience();

var app = builder.Build();
var logger = app.Logger;

app.MapGet("/mailing-lists", async (ISendPulserClient sendPulse, CancellationToken cancellationToken) =>
    await sendPulse.AddressBooks.GetAllAsync(limit: 20, cancellationToken: cancellationToken));

// SendPulse signs nothing, so the secret in the path is the authentication.
var webhookSecret = builder.Configuration["SendPulser:WebhookSecret"] ?? "change-me";

app.MapSendPulserEmailWebhook(
    "/hooks/sendpulse/email/{secret}",
    (events, _) =>
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            foreach (var received in events)
            {
                logger.LogInformation(
                    "{Event} for {Email} at {Timestamp}",
                    received.Event,
                    received.Email,
                    received.Timestamp);
            }
        }

        return Task.CompletedTask;
    },
    options => options.Secret = webhookSecret);

app.MapSendPulserSmtpWebhook(
    "/hooks/sendpulse/smtp/{secret}",
    (events, _) =>
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            foreach (var received in events.OfType<SmtpDeliveredEvent>())
            {
                logger.LogInformation(
                    "Message {MessageId} to {Recipient} answered {Code}",
                    received.MessageId,
                    received.Recipient,
                    received.ResponseCode);
            }
        }

        return Task.CompletedTask;
    },
    options => options.Secret = webhookSecret);

await app.RunAsync();
