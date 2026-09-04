# Security

## Reporting a vulnerability

Report vulnerabilities privately through
[GitHub security advisories](https://github.com/gberikov/SendPulser/security/advisories/new). Do not
open a public issue. You will get an acknowledgement within a few days and a fix or a mitigation plan
before any public disclosure.

## What the library does with your secrets

- The OAuth client secret is sent only in the token request body, only over HTTPS. `SendPulserOptions`
  refuses a plain `http://` base address unless it points at the local machine.
- Access tokens are held in memory for the lifetime of the token provider and never logged. The only
  authentication log line names the HTTP method and path of the rejected request, without the query
  string, because SendPulse query strings carry email addresses.
- `SendPulserApiException.ResponseBody` keeps the raw SendPulse response for diagnostics. It can contain
  contact email addresses; think before writing it to a shared log.
- Webhook endpoints compare the URL secret in constant time and never log it.

## Supported versions

Only the latest release receives fixes.
