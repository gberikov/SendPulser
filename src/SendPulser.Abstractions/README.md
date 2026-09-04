# SendPulser.Abstractions

Interfaces, models, options and exceptions for [SendPulser](https://github.com/gberikov/SendPulser),
the .NET client for the SendPulse email API.

Reference this package from code that only needs the contracts: it lets you depend on
`ISendPulserClient` and its models without pulling in the HTTP implementation, and makes everything
straightforward to mock in tests. It also holds the webhook event types used by `SendPulser.AspNetCore`
and the JSON converters that normalise the shapes SendPulse returns inconsistently.

To actually talk to SendPulse, install `SendPulser` or `SendPulser.DependencyInjection`.

Documentation: [index](https://github.com/gberikov/SendPulser/blob/master/docs/README.md),
[Getting started](https://github.com/gberikov/SendPulser/blob/master/docs/getting-started.md).
