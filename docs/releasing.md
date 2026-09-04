# Releasing

[Documentation index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md)

## Versioning

All five packages share one version, derived by [MinVer](https://github.com/adamralph/minver) from the
closest git tag. A tag `1.2.0` on the released commit produces packages `1.2.0`; commits after it
produce `1.2.1-alpha.0.N` until the next tag. Tags are plain versions without a `v` prefix, which is
what the CI trigger `tags: ['[0-9]*']` expects.

## Steps

1. Move the entries under `[Unreleased]` in `CHANGELOG.md` to a new version heading and add the
   comparison link at the bottom.
2. Move the contents of every `PublicAPI.Unshipped.txt` into the `PublicAPI.Shipped.txt` next to it,
   leaving only the `#nullable enable` line in the unshipped file. From then on the analyzer reports
   removals of shipped API as `*REMOVED*` entries, which is the signal for a major version bump.
3. Merge `develop` into `master`.
4. Tag: `git tag 1.2.0 && git push origin 1.2.0`.

CI builds, tests, checks the Native AOT publish, packs and pushes to nuget.org through trusted
publishing; no API key is stored in the repository. The `NUGET_USER` repository variable must name the
nuget.org account that owns the packages, and the trusted publishing policy for this repository must
exist at <https://www.nuget.org/account/trustedpublishing>.

## Regenerating the public API files

The analyzer emits one `RS0016` warning per missing symbol and one `RS0017` per stale line, and both
messages contain the exact line the file needs. When the IDE fix-all is not at hand:

```bash
dotnet build SendPulser.slnx -c Release --no-incremental -p:TreatWarningsAsErrors=false -v:q > build.log
```

Then add every `Symbol '...' is not part of the declared public API` text to the
`PublicAPI.Unshipped.txt` of the project named at the end of the line, remove every
`Symbol '...' is part of the declared API, but ...` text from it, and rebuild.

## Support policy

`net8.0` is supported until .NET 8 leaves support in November 2026; `net10.0` from then on. Both
targets are built and tested on every push. Runtime dependencies are pinned per target framework in
`Directory.Packages.props`, so `net8.0` consumers keep the 8.x `Microsoft.Extensions.*` assemblies.
