using System;
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

app.MapGet("/proxy", async (HttpContext context) =>
{
    var url = context.Request.Query["url"];

    //if (string.IsNullOrEmpty(url))
    //{
    //    return Results.BadRequest("Query parameter 'url' is required.");
    //}

    //try
    //{
    //// Validate URL
    //if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || 
    //    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    //{
    //    return Results.BadRequest("Invalid or unsafe URL.");
    //}

    //// Fetch the response
    //var response = await httpClient.GetAsync(uri);
    //response.EnsureSuccessStatusCode();

    //// Log the request
    //Console.WriteLine($"Fetched {url} with status code {response.StatusCode}");

    //// Read response content
    //var content = await response.Content.ReadAsStringAsync();
    //var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

    //return Results.Content(content, contentType);

        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var authHeaderParts = context.Request.Headers["Authorization"].ToString().Split('=', 2);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authHeaderParts[0], authHeaderParts[1]);
        }

        var response = await httpClient.GetAsync(url);


        // Set the CORS headers
        //context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        //context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        //context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

        // Forward the Content-Type header to the client
        if (response.Content.Headers.ContentType != null)
        {
            context.Response.ContentType = response.Content.Headers.ContentType.ToString();
        }

        // Return the response
        context.Response.StatusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsByteArrayAsync();

        await context.Response.Body.WriteAsync(content);
    //}
    //catch (HttpRequestException ex)
    //{
    //    return Results.Problem($"Error fetching URL: {ex.Message}");
    //}
});

app.Run();
