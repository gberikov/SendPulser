# Contributing

## Building

```bash
dotnet build SendPulser.slnx
dotnet test tests/SendPulser.Tests/SendPulser.Tests.csproj -p:CollectCoverage=true -p:Threshold=80
```

The SDK version is pinned in `global.json`; `dotnet --list-sdks` should show a 10.0 SDK. The packages
target `net8.0` and `net10.0`, both are built and tested.

## Rules the build enforces

- Warnings are errors, including the code style rules in `.editorconfig`.
- Every public symbol is tracked by `Microsoft.CodeAnalysis.PublicApiAnalyzers`. Changing anything
  public fails the build until the change is recorded in the `PublicAPI.Unshipped.txt` file of that
  package. Rider and Visual Studio offer an "Add to public API" quick fix with fix-all; see
  [docs/releasing.md](https://github.com/gberikov/SendPulser/blob/master/docs/releasing.md) for a
  scripted alternative.
- Line coverage of the implementation packages must stay at 80% or above.
- The console sample must publish with Native AOT; CI runs `dotnet publish -p:PublishAot=true`.

## Adding an endpoint

1. Add the model to `SendPulser.Abstractions`, with `[JsonPropertyName]` on every property and an XML
   doc comment. Numbers SendPulse returns as strings need no special handling; flags returned as
   `0`/`1` use `FlexibleBooleanConverter`, dates use `SendPulseDateTimeConverter`.
2. Add the method to the service interface, then implement it in `src/SendPulser/Services`.
3. Register every new request and response type in `SendPulserJsonContext`; nothing is serialized
   through reflection.
4. Add a test in `tests/SendPulser.Tests` that asserts the path, the query string, the request body and
   the parsed response, using a fixture copied from the SendPulse documentation.
5. Document the method in the matching page under `docs/`.

## Pull requests

Target `develop`. Keep the CHANGELOG entry under `[Unreleased]` in the same pull request.
