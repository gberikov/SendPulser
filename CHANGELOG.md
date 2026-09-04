# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html). All five packages share one version.

## [Unreleased]

## [0.1.0]

First release. The library was rewritten from scratch; it shares no code and no API with the
`SendPulse.Net` package, which is a different project.

### Added

- `SendPulser.Abstractions`: contracts, models, options and exceptions, including converters for the
  shapes SendPulse returns inconsistently (timestamps without a timezone, tags as an object or an
  array, category info as an empty array, numbers as strings).
- `SendPulser`: the client, targeting `net8.0`, with source generated JSON and Native AOT support.
  Mailing lists and contacts, templates, campaigns, transactional email and webhook registration.
  OAuth tokens are cached per client and refreshed a minute before they expire; a request rejected
  with 401 is replayed once with a fresh token.
- `SendPulser.DependencyInjection`: `AddSendPulser(...)` over `IHttpClientFactory`, with configuration
  read key by key so the package stays AOT safe.
- `SendPulser.Resilience`: client-side limiter for the 10 requests per second ceiling, attempt timeout
  and retries that never replay a write.
- `SendPulser.AspNetCore`: `MapSendPulserEmailWebhook` and `MapSendPulserSmtpWebhook`, with a
  constant-time URL secret, an optional address allowlist and pass-through of unknown event types.

[Unreleased]: https://github.com/gberikov/SendPulser/compare/0.1.0...HEAD
[0.1.0]: https://github.com/gberikov/SendPulser/releases/tag/0.1.0
