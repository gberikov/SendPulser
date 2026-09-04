using SendPulser.Balance;
using SendPulser.Internal;

namespace SendPulser.Services;

internal sealed class BalanceService(SendPulserApi api) : IBalanceService
{
    private readonly SendPulserApi _api = api;

    public Task<AccountBalance> GetAsync(string? currency = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(currency)
            ? "balance"
            : "balance/" + SendPulserApi.Segment(currency.ToUpperInvariant());

        return _api.GetAsync(path, SendPulserJsonContext.Default.AccountBalance, cancellationToken);
    }

    public Task<BalanceDetails> GetDetailsAsync(CancellationToken cancellationToken = default) =>
        _api.GetAsync("user/balance/detail", SendPulserJsonContext.Default.BalanceDetails, cancellationToken);
}
