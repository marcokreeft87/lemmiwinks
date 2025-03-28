using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Bypass SSL validation (for development only)
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
var httpClient = new HttpClient(handler);

// Enable CORS for all origins, methods, and headers
app.UseCors("AllowAll");

app.MapGet("/proxy", async (HttpContext context, ILogger<Program> logger) =>
{
    var url = context.Request.Query["url"];

    logger.LogInformation($"Proxying request to {url}. IP: {context.Connection.RemoteIpAddress?.ToString()}");

    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var authHeaderParts = context.Request.Headers["Authorization"].ToString().Split('=', 2);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authHeaderParts[0], authHeaderParts[1]);
    }

    var response = await httpClient.GetAsync(url);

    logger.LogInformation($"Received response from {url} with status code {response.StatusCode}");

    // Forward the Content-Type header to the client
    if (response.Content.Headers.ContentType != null)
    {
        context.Response.ContentType = response.Content.Headers.ContentType.ToString();
    }

    // Return the response
    context.Response.StatusCode = (int)response.StatusCode;
    var content = await response.Content.ReadAsByteArrayAsync();

    await context.Response.Body.WriteAsync(content);
});

app.Run();
