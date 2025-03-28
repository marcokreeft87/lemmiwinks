var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors();

var app = builder.Build();

// Bypass SSL validation (for development only)
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
var httpClient = new HttpClient(handler);

// Enable CORS for all origins, methods, and headers
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.MapGet("/proxy", async (HttpContext context) =>
{
    var url = context.Request.Query["url"];

    if (string.IsNullOrEmpty(url))
    {
        return Results.BadRequest("Query parameter 'url' is required.");
    }

    try
    {
        // Validate URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || 
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Results.BadRequest("Invalid or unsafe URL.");
        }

        // Fetch the response
        var response = await httpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();

        // Log the request
        Console.WriteLine($"Fetched {url} with status code {response.StatusCode}");

        // Read response content
        var content = await response.Content.ReadAsStringAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        return Results.Content(content, contentType);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem($"Error fetching URL: {ex.Message}");
    }
});

app.Run();
