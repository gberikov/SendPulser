using System.Net;
using SendPulser.AddressBooks;
using SendPulser.Tests.Infrastructure;

namespace SendPulser.Tests;

public class AddressBookServiceTests
{
    [Fact]
    public async Task Lists_mailing_lists_with_paging_passed_straight_through()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("address-books.json"));

        var books = await client.AddressBooks.GetAllAsync(limit: 10, offset: 5);

        Assert.Equal(2, books.Count);
        Assert.Equal("My first book", books[0].Name);
        Assert.Equal("/addressbooks", handler.LastRequest.Path);
        Assert.Equal("?limit=10&offset=5", handler.LastRequest.Query);
    }

    [Fact]
    public async Task Omits_paging_parameters_that_were_not_given()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("[]");

        await client.AddressBooks.GetAllAsync();

        Assert.Equal(string.Empty, handler.LastRequest.Query);
    }

    [Fact]
    public async Task Unwraps_the_single_element_array_a_mailing_list_arrives_in()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("address-books.json"));

        var book = await client.AddressBooks.GetAsync(1);

        Assert.Equal(1, book.Id);
        Assert.Equal("/addressbooks/1", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reports_a_missing_mailing_list_as_a_not_found_failure()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("[]");

        var exception = await Assert.ThrowsAsync<SendPulserApiException>(() => client.AddressBooks.GetAsync(404));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task Creates_a_mailing_list_and_returns_its_id()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"id":422325}""");

        var id = await client.AddressBooks.CreateAsync("My New Book");

        Assert.Equal(422325, id);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("""{"bookName":"My New Book"}""", handler.LastRequest.Body);
    }

    [Fact]
    public async Task Renames_a_mailing_list_with_a_put()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.AddressBooks.RenameAsync(7, "New Name");

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/addressbooks/7", handler.LastRequest.Path);
        Assert.Equal("""{"name":"New Name"}""", handler.LastRequest.Body);
    }

    [Fact]
    public async Task Deletes_a_mailing_list()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.AddressBooks.DeleteAsync(7);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/addressbooks/7", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_contacts_and_their_variables()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith(TestClient.Fixture("contacts.json"));

        var contacts = await client.AddressBooks.GetContactsAsync(1, limit: 100);

        Assert.Equal(2, contacts.Count);
        Assert.Equal("test@test.com", contacts[0].Email);
        Assert.Equal("/addressbooks/1/emails", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Reads_the_contact_count()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"total":42}""");

        Assert.Equal(42, await client.AddressBooks.GetContactCountAsync(1));
        Assert.Equal("/addressbooks/1/emails/total", handler.LastRequest.Path);
    }

    [Fact]
    public async Task Adds_contacts_with_variables_and_tags()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.AddressBooks.AddContactsAsync(
            1,
            [new NewContact("a@b.com") { Variables = new Dictionary<string, string> { ["Name"] = "Ann" } }],
            tagIds: [3456]);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            """{"emails":[{"email":"a@b.com","variables":{"Name":"Ann"}}],"tags":[3456]}""",
            handler.LastRequest.Body);
    }

    [Fact]
    public async Task Adds_contacts_with_double_opt_in()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.AddressBooks.AddContactsWithConfirmationAsync(
            1,
            [new NewContact("a@b.com")],
            new DoubleOptInSettings("sender@example.com", "en") { TemplateId = "abc" });

        var body = handler.LastRequest.Body!;
        Assert.Contains("\"confirmation\":\"force\"", body, StringComparison.Ordinal);
        Assert.Contains("\"sender_email\":\"sender@example.com\"", body, StringComparison.Ordinal);
        Assert.Contains("\"message_lang\":\"en\"", body, StringComparison.Ordinal);
        Assert.Contains("\"template_id\":\"abc\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Deletes_contacts_with_the_addresses_in_the_body()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""{"result":true}""");

        await client.AddressBooks.DeleteContactsAsync(1, ["a@b.com", "c@d.com"]);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/addressbooks/1/emails", handler.LastRequest.Path);
        Assert.Equal("""{"emails":["a@b.com","c@d.com"]}""", handler.LastRequest.Body);
    }

    [Fact]
    public async Task Reads_the_variables_of_a_mailing_list()
    {
        var (client, handler) = TestClient.Create();
        using var _ = client;
        handler.RespondWith("""[{"name":"email","type":"string"},{"name":"code","type":"number"}]""");

        var variables = await client.AddressBooks.GetVariablesAsync(1);

        Assert.Equal(2, variables.Count);
        Assert.Equal("number", variables[1].Type);
        Assert.Equal("/addressbooks/1/variables", handler.LastRequest.Path);
    }
}
