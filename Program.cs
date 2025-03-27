var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var httpClient = new HttpClient();

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
