# Errors and resilience

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

## Exceptions

Everything the library raises derives from `SendPulserException`.

| Exception | Raised when |
| --- | --- |
| `SendPulserApiException` | SendPulse answered and the answer is a failure: a non-2xx status, an `is_error` payload returned with HTTP 200, a `result: false` or `success: false` flag, or a body that does not match the documented shape. Carries `StatusCode`, the SendPulse `ErrorCode` when present and the raw `ResponseBody`. |
| `SendPulserAuthenticationException` | The token endpoint refused the credentials or returned no token. Derives from `SendPulserApiException`. |
| `SendPulserTransportException` | No SendPulse answer was produced: connection failure, attempt timeout or rejection by the client-side rate limiter. The cause is in `InnerException`. |

Argument validation still throws the usual `ArgumentException` family, and cancellation still throws
`OperationCanceledException`. The overall `HttpClient.Timeout` surfaces as `TaskCanceledException`
with a `TimeoutException` inside, as it does for any `HttpClient`.

SendPulse reports part of its failures with HTTP 200 and a body such as
`{"is_error": true, "error_code": 301, "message": "Wrong parameters"}`. The client detects that and
raises `SendPulserApiException` instead of returning a half-empty object. The documented error codes are
listed on the [SendPulse API page](https://sendpulse.com/integrations/api#errors); the one worth
knowing is `2020202020`, the hard limit of ten requests per second, exposed as
`RateLimit.PerSecondErrorCode`.

## Rate limits

SendPulse enforces a ceiling of ten requests per second on every plan, plus a per-minute quota that
depends on the plan and is answered with HTTP 429. Campaign creation is further limited to four per hour
(error code 791), and SMTP resubscription emails to five per day.

## The resilience package

```csharp
builder.Services
    .AddSendPulser(configuration.GetSection("SendPulser"))
    .AddSendPulserResilience(options =>
    {
        options.RequestsPerSecond = 5;   // lower it when several processes share one account
        options.MaxRetryAttempts = 3;
    });
```

`AddSendPulserResilience()` adds, from the outside in:

1. A translation step that turns Polly's `TimeoutRejectedException` and `RateLimiterRejectedException`
   into `SendPulserTransportException`.
2. A sliding window rate limiter for the ten requests per second ceiling. Requests beyond
   `QueueLimit` (64) waiting for a slot are rejected rather than queued forever.
3. Retries with exponential backoff and jitter.
4. A per attempt timeout.

| Option | Default |
| --- | --- |
| `RequestsPerSecond` | 10 |
| `QueueLimit` | 64 |
| `MaxRetryAttempts` | 3 |
| `RetryDelay` | 1 second |
| `AttemptTimeout` | 30 seconds |

**Writes are never retried.** Sending a campaign or a transactional email is not idempotent, so a
replayed `POST` can deliver the same email twice; only `GET` and `DELETE` are replayed after a
connection failure, an attempt timeout, a 5xx or a 408. HTTP 429 is retried for every method, honouring
`Retry-After`, because the request was rejected rather than processed.

The limiter is per process. When several processes share one SendPulse account, divide the ceiling
between them with `RequestsPerSecond`.

## Without the resilience package

The self-contained `SendPulserClient` and a bare `AddSendPulser()` registration have no limiter and no
retries. `HttpRequestException` is still wrapped in `SendPulserTransportException`, so catching
`SendPulserException` covers every failure either way.
