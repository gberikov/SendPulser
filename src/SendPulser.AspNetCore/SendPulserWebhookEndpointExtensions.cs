using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SendPulser.AspNetCore;
using SendPulser.AspNetCore.Internal;
using SendPulser.Webhooks;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Maps endpoints that receive SendPulse webhooks.
/// </summary>
public static class SendPulserWebhookEndpointExtensions
{
    /// <summary>
    /// Maps an endpoint that receives bulk email service events.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder.</param>
    /// <param name="pattern">
    /// Route pattern. Include a <c>{secret}</c> segment when
    /// <see cref="SendPulserWebhookOptions.Secret"/> is set, for example <c>/hooks/sendpulse/{secret}</c>.
    /// </param>
    /// <param name="handler">
    /// Callback receiving one batch of events. SendPulse posts up to 100 events per request, so the whole
    /// batch is handed over at once; deciding what to do with a partial failure is the caller's job.
    /// </param>
    /// <param name="configure">Callback that sets the secret and the address allowlist.</param>
    /// <returns>The mapped endpoint, so further metadata can be added.</returns>
    /// <remarks>
    /// The endpoint answers 204 when the callback returns, 500 when it throws, 400 when the body is not
    /// JSON, 413 when it exceeds <see cref="SendPulserWebhookOptions.MaxRequestBodySize"/> and 404 when
    /// the secret does not match.
    /// </remarks>
    public static IEndpointConventionBuilder MapSendPulserEmailWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<IReadOnlyList<EmailWebhookEvent>, CancellationToken, Task> handler,
        Action<SendPulserWebhookOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return endpoints.MapSendPulserEmailWebhook(
            pattern,
            (events, _, cancellationToken) => handler(events, cancellationToken),
            configure);
    }

    /// <summary>
    /// Maps an endpoint that receives bulk email service events and hands the callback the
    /// <see cref="HttpContext"/>, so scoped services can be resolved from
    /// <see cref="HttpContext.RequestServices"/>.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder.</param>
    /// <param name="pattern">Route pattern, see <see cref="MapSendPulserEmailWebhook(IEndpointRouteBuilder, string, Func{IReadOnlyList{EmailWebhookEvent}, CancellationToken, Task}, Action{SendPulserWebhookOptions}?)"/>.</param>
    /// <param name="handler">Callback receiving one batch of events, the request context and its cancellation token.</param>
    /// <param name="configure">Callback that sets the secret and the address allowlist.</param>
    /// <returns>The mapped endpoint, so further metadata can be added.</returns>
    public static IEndpointConventionBuilder MapSendPulserEmailWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<IReadOnlyList<EmailWebhookEvent>, HttpContext, CancellationToken, Task> handler,
        Action<SendPulserWebhookOptions>? configure = null) =>
        Map(endpoints, pattern, WebhookEventParser.ParseEmailEvents, handler, configure, "email");

    /// <summary>
    /// Maps an endpoint that receives SMTP service events.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder.</param>
    /// <param name="pattern">
    /// Route pattern. Include a <c>{secret}</c> segment when
    /// <see cref="SendPulserWebhookOptions.Secret"/> is set.
    /// </param>
    /// <param name="handler">
    /// Callback receiving one batch of events. SendPulse posts up to 500 SMTP events per request.
    /// </param>
    /// <param name="configure">Callback that sets the secret and the address allowlist.</param>
    /// <returns>The mapped endpoint, so further metadata can be added.</returns>
    public static IEndpointConventionBuilder MapSendPulserSmtpWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<IReadOnlyList<SmtpWebhookEvent>, CancellationToken, Task> handler,
        Action<SendPulserWebhookOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return endpoints.MapSendPulserSmtpWebhook(
            pattern,
            (events, _, cancellationToken) => handler(events, cancellationToken),
            configure);
    }

    /// <summary>
    /// Maps an endpoint that receives SMTP service events and hands the callback the
    /// <see cref="HttpContext"/>, so scoped services can be resolved from
    /// <see cref="HttpContext.RequestServices"/>.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder.</param>
    /// <param name="pattern">Route pattern, see <see cref="MapSendPulserSmtpWebhook(IEndpointRouteBuilder, string, Func{IReadOnlyList{SmtpWebhookEvent}, CancellationToken, Task}, Action{SendPulserWebhookOptions}?)"/>.</param>
    /// <param name="handler">Callback receiving one batch of events, the request context and its cancellation token.</param>
    /// <param name="configure">Callback that sets the secret and the address allowlist.</param>
    /// <returns>The mapped endpoint, so further metadata can be added.</returns>
    public static IEndpointConventionBuilder MapSendPulserSmtpWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<IReadOnlyList<SmtpWebhookEvent>, HttpContext, CancellationToken, Task> handler,
        Action<SendPulserWebhookOptions>? configure = null) =>
        Map(endpoints, pattern, WebhookEventParser.ParseSmtpEvents, handler, configure, "smtp");

    private static IEndpointConventionBuilder Map<TEvent>(
        IEndpointRouteBuilder endpoints,
        string pattern,
        Func<JsonDocument, List<TEvent>> parse,
        Func<IReadOnlyList<TEvent>, HttpContext, CancellationToken, Task> handler,
        Action<SendPulserWebhookOptions>? configure,
        string family)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(handler);

        var options = new SendPulserWebhookOptions();
        configure?.Invoke(options);

        // A RequestDelegate is used rather than a route handler lambda: minimal API parameter binding
        // reflects over the delegate, which would make the package unusable under Native AOT.
        RequestDelegate endpoint = context => HandleAsync(context, options, parse, handler, family);

        return endpoints.MapPost(pattern, endpoint);
    }

    private static async Task HandleAsync<TEvent>(
        HttpContext context,
        SendPulserWebhookOptions options,
        Func<JsonDocument, List<TEvent>> parse,
        Func<IReadOnlyList<TEvent>, HttpContext, CancellationToken, Task> handler,
        string family)
    {
        var logger = context.RequestServices.GetService<ILoggerFactory>()
            ?.CreateLogger("SendPulser.Webhooks") ?? NullLogger.Instance;

        if (!IsAuthorized(context, options))
        {
            WebhookLog.Rejected(logger, family, context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (options.MaxRequestBodySize is { } limit)
        {
            if (context.Request.ContentLength > limit)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                return;
            }

            var sizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (sizeFeature is { IsReadOnly: false })
            {
                sizeFeature.MaxRequestBodySize = limit;
            }
        }

        JsonDocument document;
        try
        {
            document = await JsonDocument
                .ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted)
                .ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            WebhookLog.MalformedPayload(logger, family, exception);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            return;
        }

        using (document)
        {
            var events = parse(document);

            try
            {
                await handler(events, context, context.RequestAborted).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // The handler is caller supplied: any failure has to become a 500 rather than an unhandled exception.
            catch (Exception exception)
#pragma warning restore CA1031
            {
                WebhookLog.HandlerFailed(logger, family, events.Count, exception);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            WebhookLog.Handled(logger, family, events.Count);
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }
    }

    private static bool IsAuthorized(HttpContext context, SendPulserWebhookOptions options)
    {
        if (options.AllowedAddresses.Count > 0 && !IsAllowedAddress(context, options))
        {
            return false;
        }

        if (string.IsNullOrEmpty(options.Secret))
        {
            return true;
        }

        var supplied = context.Request.RouteValues.TryGetValue("secret", out var routeValue)
            ? routeValue?.ToString()
            : context.Request.Query["secret"].FirstOrDefault();

        return supplied is not null && FixedTimeEquals(supplied, options.Secret);
    }

    private static bool IsAllowedAddress(HttpContext context, SendPulserWebhookOptions options)
    {
        var remote = context.Connection.RemoteIpAddress;
        if (remote is null)
        {
            return false;
        }

        if (remote.IsIPv4MappedToIPv6)
        {
            remote = remote.MapToIPv4();
        }

        foreach (var allowed in options.AllowedAddresses)
        {
            var candidate = allowed.IsIPv4MappedToIPv6 ? allowed.MapToIPv4() : allowed;
            if (candidate.Equals(remote))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Compares two secrets without leaking their length or their matching prefix through timing.
    /// </summary>
    private static bool FixedTimeEquals(string supplied, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(supplied)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
}
