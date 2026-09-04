# SendPulser.Abstractions

Interfaces, models, options and exceptions for [SendPulser](https://github.com/gberikov/SendPulser),
the .NET client for the SendPulse email API.

Reference this package from code that only needs the contracts: it lets you depend on
`ISendPulserClient` and its models without pulling in the HTTP implementation, and makes everything
straightforward to mock in tests.

To actually talk to SendPulse, install `SendPulser` or `SendPulser.DependencyInjection`.
