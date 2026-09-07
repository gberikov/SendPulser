# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html). All five packages share one version.

## [Unreleased]

## [0.2.0]

### Added

- Campaign AMP bodies, binary attachments, statistics settings and UTM campaign values; complete
  sender-domain fields; and control over the SMTP message-list `country` flag.

### Fixed

- Preserve malformed known webhook events through the unknown-event fallback, reject invalid batch
  items, and require a configured secret whenever the route declares `{secret}`, including route-group
  prefixes and catch-all parameters.
- Serialize custom campaign UTM values inside `stats` through `Statistics.UtmCampaign`.
- Send webhook registration bodies as form data, validate typed success responses, rate-limit every
  retry attempt, and publish cached OAuth token state atomically.

## [0.1.0]

First release. The library was rewritten from scratch; it shares no code and no API with the
`SendPulse.Net` package, which is a different project.

### Added

- `SendPulser.Abstractions`: contracts, models, options and exceptions for the whole documented surface
  of the SendPulse bulk email and SMTP services: mailing lists and contacts, email address lookups,
  campaigns with country, referral and per recipient statistics, templates, senders, blacklist, tags,
  balance, transactional email with tracking, bounces, the unsubscribe list and sender domains, and
  webhook registrations. Status codes are exposed through the `ContactStatus`, `CampaignStatus` and
  `DeliveryStatus` constants. Converters normalise the shapes SendPulse returns inconsistently:
  timestamps without a time zone, tags as an object or an array, category info as an empty array,
  numbers as strings, flags as `0`/`1`, contact variables as an object or an empty array.
- `SendPulser`: the client, targeting `net8.0` and `net10.0`, with source generated JSON and Native AOT
  support. OAuth tokens are cached per client and refreshed a minute before they expire; a request
  rejected with 401 is replayed once with a fresh token. Bodies are handled as UTF-8 bytes end to end.
  Connection failures surface as `SendPulserTransportException`; `is_error`, `result: false` and
  `success: false` answers, including those inside the `data` envelopes of the v2 endpoints, surface as
  `SendPulserApiException`.
- `SendPulser.DependencyInjection`: `AddSendPulser(...)` over `IHttpClientFactory`, registering the
  client as a singleton on a `SocketsHttpHandler` with connection recycling. Configuration is read key by
  key so the package stays AOT safe; `BaseAddress`, `Timeout` and `TokenRefreshMargin` are all bindable.
- `SendPulser.Resilience`: client-side limiter for the 10 requests per second ceiling, attempt timeout
  and retries that never replay a write. Attempt timeouts are retried like connection failures, and
  Polly's rejection exceptions are translated into `SendPulserTransportException`.
- `SendPulser.AspNetCore`: `MapSendPulserEmailWebhook` and `MapSendPulserSmtpWebhook`, with a
  constant-time URL secret, an optional address allowlist, a request body size limit and overloads that
  hand the `HttpContext` to the callback. Every documented event of both services is typed, including
  bounces, spam reports, campaign status changes, deletions and undelivered messages; unknown events and
  unknown fields are preserved.
- Documentation split into pages under `docs/`, linked with absolute URLs so the package READMEs read
  correctly on nuget.org.

[Unreleased]: https://github.com/gberikov/SendPulser/compare/0.2.0...HEAD
[0.2.0]: https://github.com/gberikov/SendPulser/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/gberikov/SendPulser/releases/tag/0.1.0
