using SendPulser;

// Credentials come from the environment so nothing secret ends up in the repository:
//   setx SENDPULSE_CLIENT_ID     your-id
//   setx SENDPULSE_CLIENT_SECRET your-secret
var clientId = Environment.GetEnvironmentVariable("SENDPULSE_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("SENDPULSE_CLIENT_SECRET");

if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
{
    Console.Error.WriteLine("Set SENDPULSE_CLIENT_ID and SENDPULSE_CLIENT_SECRET first.");
    return 1;
}

using var client = new SendPulserClient(clientId, clientSecret);

try
{
    var balance = await client.Balance.GetAsync();
    Console.WriteLine($"Balance: {balance.Amount} {balance.Currency}");

    var senders = await client.Senders.GetAllAsync();
    Console.WriteLine($"Senders: {senders.Count}");
    foreach (var sender in senders)
    {
        Console.WriteLine($"  {sender.Email,-40} {sender.Status} (SMTP: {sender.IsAllowedForSmtp})");
    }

    var books = await client.AddressBooks.GetAllAsync(limit: 10);
    Console.WriteLine($"Mailing lists: {books.Count}");
    foreach (var book in books)
    {
        Console.WriteLine($"  {book.Id,10}  {book.Name} ({book.AllEmailCount} contacts, {book.StatusExplanation})");
    }

    var templates = await client.Templates.GetAllAsync(owner: "me");
    Console.WriteLine($"Own templates: {templates.Count}");
    foreach (var template in templates.Take(10))
    {
        Console.WriteLine($"  {template.RealId,10}  {template.Name}");
    }
}
catch (SendPulserAuthenticationException exception)
{
    Console.Error.WriteLine($"SendPulse refused the credentials: {exception.Message}");
    return 2;
}
catch (SendPulserException exception)
{
    // Covers API failures (SendPulserApiException) and transport failures (SendPulserTransportException).
    Console.Error.WriteLine(exception.Message);
    return 3;
}

return 0;
