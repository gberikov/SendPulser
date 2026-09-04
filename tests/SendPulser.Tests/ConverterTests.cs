using System.Text.Json;
using SendPulser.AddressBooks;
using SendPulser.Campaigns;
using SendPulser.Internal;
using SendPulser.Smtp;
using SendPulser.Templates;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

/// <summary>
/// Exercises the converters through the very serializer context the client uses, so the tests cannot
/// drift from the settings that ship.
/// </summary>
public class ConverterTests
{
    private static readonly SendPulserJsonContext Json = SendPulserJsonContext.Default;

    [Fact]
    public void Reads_the_space_separated_timestamps_SendPulse_returns()
    {
        var books = JsonSerializer.Deserialize(TestClient.Fixture("address-books.json"), Json.ListAddressBook)!;

        Assert.Equal(new DateTime(2015, 4, 20, 8, 52, 40, DateTimeKind.Unspecified), books[0].CreatedAt);
    }

    [Fact]
    public void Treats_an_empty_timestamp_as_no_value()
    {
        var templates = JsonSerializer.Deserialize(TestClient.Fixture("templates.json"), Json.ListTemplate)!;

        Assert.Null(templates[1].CreatedAt);
    }

    [Fact]
    public void Reads_tags_whether_they_arrive_as_an_object_or_an_array()
    {
        var templates = JsonSerializer.Deserialize(TestClient.Fixture("templates.json"), Json.ListTemplate)!;

        Assert.Equal(["webinar", "study"], templates[0].Tags);
        Assert.Empty(templates[1].Tags);
    }

    [Fact]
    public void Reads_category_info_as_null_when_SendPulse_sends_an_empty_array()
    {
        var templates = JsonSerializer.Deserialize(TestClient.Fixture("templates.json"), Json.ListTemplate)!;

        Assert.Equal("Education", templates[0].CategoryInfo?.Name);
        Assert.Null(templates[1].CategoryInfo);
    }

    [Fact]
    public void Reads_numbers_that_arrive_as_strings()
    {
        var emails = JsonSerializer.Deserialize(TestClient.Fixture("smtp-emails.json"), Json.ListSmtpEmail)!;

        Assert.Equal(1128, emails[0].TotalSize);
        Assert.Equal(250, emails[0].SmtpAnswerCode);
    }

    [Fact]
    public void Keeps_contact_variables_of_mixed_types()
    {
        var contacts = JsonSerializer.Deserialize(TestClient.Fixture("contacts.json"), Json.ListContact)!;

        Assert.Equal("John", contacts[0].Variables!["name"]);
        Assert.Equal("42", contacts[0].Variables!["code"]);
        Assert.Null(contacts[1].Variables);
    }

    [Fact]
    public void Encodes_the_body_as_Base64_on_the_way_out()
    {
        var json = JsonSerializer.Serialize(
            new CreateTemplateRequest { Body = "<p>hi</p>", Language = "en" },
            Json.CreateTemplateRequest);

        Assert.Equal("PHA+aGk8L3A+", ReadProperty(json, "body"));
    }

    [Fact]
    public void Decodes_a_Base64_body_on_the_way_in()
    {
        var request = JsonSerializer.Deserialize("""{"body":"PHA+aGk8L3A+"}""", Json.CreateTemplateRequest)!;

        Assert.Equal("<p>hi</p>", request.Body);
    }

    [Fact]
    public void Leaves_a_body_that_is_not_Base64_untouched_when_reading()
    {
        // Campaign details come back as plain HTML rather than Base64.
        var request = JsonSerializer.Deserialize("""{"body":"<p>plain</p>"}""", Json.CreateTemplateRequest)!;

        Assert.Equal("<p>plain</p>", request.Body);
    }

    [Fact]
    public void Serializes_a_single_mailing_list_as_a_number_and_several_as_an_array()
    {
        var single = JsonSerializer.Serialize(
            new CreateCampaignRequest { ListIds = [123] },
            Json.CreateCampaignRequest);
        var many = JsonSerializer.Serialize(
            new CreateCampaignRequest { ListIds = [123, 456] },
            Json.CreateCampaignRequest);

        Assert.Contains("\"list_id\":123", single, StringComparison.Ordinal);
        Assert.Contains("\"list_id\":[123,456]", many, StringComparison.Ordinal);
    }

    [Fact]
    public void Writes_binary_attachments_as_Base64()
    {
        var envelope = new SendEmailEnvelope
        {
            Email = new SendEmailRequest
            {
                Subject = "hi",
                BinaryAttachments = new Dictionary<string, byte[]> { ["a.txt"] = "my text"u8.ToArray() },
            },
        };

        var json = JsonSerializer.Serialize(envelope, Json.SendEmailEnvelope);

        Assert.Contains("\"attachments_binary\":{\"a.txt\":\"bXkgdGV4dA==\"}", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Omits_optional_values_that_were_not_set()
    {
        var json = JsonSerializer.Serialize(
            new CreateTemplateRequest { Body = "<p>hi</p>" },
            Json.CreateTemplateRequest);

        Assert.DoesNotContain("\"name\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"lang\"", json, StringComparison.Ordinal);
    }

    private static string? ReadProperty(string json, string name)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(name).GetString();
    }
}
