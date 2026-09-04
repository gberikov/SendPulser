using System.Text.Json;

namespace SendPulser.Tests.Infrastructure;

/// <summary>
/// Reads a property out of a recorded request body. Comparing parsed values rather than raw text keeps
/// the assertions readable: System.Text.Json escapes the plus signs of Base64 as +.
/// </summary>
internal static class JsonBody
{
    public static string? Property(string? body, string name)
    {
        using var document = JsonDocument.Parse(body!);
        return document.RootElement.GetProperty(name).GetString();
    }

    public static string? Property(string? body, string outer, string inner)
    {
        using var document = JsonDocument.Parse(body!);
        return document.RootElement.GetProperty(outer).GetProperty(inner).GetString();
    }
}
