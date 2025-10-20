using Microsoft.Extensions.Configuration;
using SendPulse.Client;
using SendPulse.Client.Common.Entities;
using SendPulse.Client.Common.Exceptions;
using SendPulse.Client.Templates.Entities;

Console.WriteLine("=== SendPulse API Client Test ===\n");

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Get credentials from configuration
var clientId = configuration["SendPulse:ClientId"] 
    ?? throw new InvalidOperationException("SendPulse:ClientId not found in appsettings.json");
var clientSecret = configuration["SendPulse:ClientSecret"] 
    ?? throw new InvalidOperationException("SendPulse:ClientSecret not found in appsettings.json");
var baseUrl = configuration["SendPulse:BaseUrl"] ?? "https://api.sendpulse.com";

Console.WriteLine("✓ Configuration loaded from appsettings.json");

try
{
    // Create client
    using var client = new SendPulseClient(clientId, clientSecret, baseUrl);
    Console.WriteLine("✓ SendPulse client created successfully");
    Console.WriteLine($"  Base URL: {client.BaseUrl}\n");

    // Test 1: Authentication
    Console.WriteLine("--- Test 1: Authentication ---");
    try
    {
        // Authentication happens automatically on first request
        Console.WriteLine("Authenticating...");
        
        // Get only user templates (excluding SendPulse templates)
        var templates = await client.Templates.GetAsync("me");
        Console.WriteLine("✓ Authentication successful!\n");

        // Test 2: Get list of templates
        Console.WriteLine("--- Test 2: Get list of templates (mine only) ---");
        Console.WriteLine($"Found templates: {templates.Count}\n");

        if (templates.Count > 0)
        {
            Console.WriteLine("Template list:");
            foreach (var template in templates)
            {
                Console.WriteLine($"  • ID: {template.Id}");
                Console.WriteLine($"    Real ID: {template.RealId}");
                Console.WriteLine($"    Name: {template.Name}");
                Console.WriteLine($"    Created: {template.Created}");
                Console.WriteLine($"    Category: {template.Category}");
                
                if (template.CategoryInfo != null)
                {
                    Console.WriteLine($"    Category info:");
                    Console.WriteLine($"      - Name: {template.CategoryInfo.Name}");
                    Console.WriteLine($"      - Code: {template.CategoryInfo.Code}");
                }
                
                if (template.Tags.Count > 0)
                {
                    Console.WriteLine($"    Tags: {string.Join(", ", template.Tags.Select(t => $"{t.Name} ({t.Id})"))}");
                }
                
                Console.WriteLine($"    Owner: {template.Owner}");
                
                if (template.Preview != null)
                {
                    Console.WriteLine($"    Preview: {template.Preview}");
                }
                
                Console.WriteLine();
            }

            // Test 3: Get specific template
            // if (templates.Count > 0)
            // {
            //     var firstTemplateId = templates[0].Id;
            //     Console.WriteLine($"--- Test 3: Get template ID={firstTemplateId} ---");
            //     var template = await client.Templates.GetByIdAsync(firstTemplateId);
            //
            //     if (template != null)
            //     {
            //         Console.WriteLine("✓ Template retrieved successfully:");
            //         Console.WriteLine($"  ID: {template.Id}");
            //         Console.WriteLine($"  Name: {template.Name}");
            //         Console.WriteLine($"  Preview: {template.Preview}");
            //     }
            // }
        }
        else
        {
            Console.WriteLine("⚠ No templates found. Create at least one template in SendPulse.");
        }
        
        // Additional test: get all templates (without filter)
        Console.WriteLine("\n--- Additional test: All templates (including SendPulse) ---");
        var allTemplates = await client.Templates.GetAsync();
        Console.WriteLine($"Total templates (including SendPulse): {allTemplates.Count}");
        Console.WriteLine($"  - My templates: {allTemplates.Count(t => t.Owner == "you")}");
        Console.WriteLine($"  - SendPulse templates: {allTemplates.Count(t => t.Owner == "sendpulse")}");

        // Test 4: Create new template
        Console.WriteLine("\n--- Test 4: Create new template ---");
        var newTemplate = new CreateTemplateRequest
        {
            Name = $"Test template {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Body = "<html><body><h1>Hello!</h1><p>This is a test template created via API.</p></body></html>",
            Language = Language.English
        };

        var createResponse = await client.Templates.CreateAsync(newTemplate);
        if (createResponse is { Result: true, RealId: > 0 })
        {
            Console.WriteLine($"✓ Template created successfully!");
            Console.WriteLine($"  Real ID: {createResponse.RealId}");

            // Test 5: Update template
            Console.WriteLine("\n--- Test 5: Update template ---");
            var updateRequest = new UpdateTemplateRequest
            {
                Body = "<html><body><h1>Updated content!</h1><p>Template was updated via API.</p></body></html>",
                Language = Language.English
            };

            var updateResponse = await client.Templates.EditAsync(createResponse.RealId, updateRequest);
            if (updateResponse.Result)
            {
                Console.WriteLine("✓ Template updated successfully!");
            }
        }
        else
        {
            Console.WriteLine("✗ Failed to create template");
        }
    }
    catch (SendPulseException ex)
    {
        Console.WriteLine($"✗ SendPulse API error: {ex.Message}");
    }

    Console.WriteLine("\n=== All tests completed ===");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Critical error: {ex.Message}");
    Console.WriteLine($"Error type: {ex.GetType().Name}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner error: {ex.InnerException.Message}");
    }
}
