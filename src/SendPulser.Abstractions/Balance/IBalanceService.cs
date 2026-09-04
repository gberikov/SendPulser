namespace SendPulser.Balance;

/// <summary>
/// Balance and plan information of the account.
/// </summary>
/// <remarks>
/// Wraps the <c>/balance</c> and <c>/user/balance/detail</c> endpoints of
/// <see href="https://sendpulse.com/integrations/api/bulk-email">the bulk email service API</see>.
/// </remarks>
public interface IBalanceService
{
    /// <summary>Gets the account balance.</summary>
    /// <param name="currency">Currency code to report in, for example <c>USD</c>; the account currency when omitted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The balance.</returns>
    Task<AccountBalance> GetAsync(string? currency = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the balance together with the plan of every service in use.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The details.</returns>
    Task<BalanceDetails> GetDetailsAsync(CancellationToken cancellationToken = default);
}
