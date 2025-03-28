# Lemmiwinks

This application implements a **proxy service** that forwards HTTP requests to a target URL and returns the response back to the client. The proxy supports CORS (Cross-Origin Resource Sharing) for all origins, methods, and headers, and includes optional **Authorization** header forwarding for authentication. It also bypasses SSL certificate validation, making it suitable for development environments.

## Features

- **CORS Enabled**: The API allows cross-origin requests from any domain (for development purposes).
- **Authorization Header Forwarding**: If the incoming request includes an `Authorization` header, it forwards this header to the target URL.
- **SSL Validation Bypass**: SSL validation is bypassed (only for development purposes). This is useful when working with self-signed certificates or during local development.
- **Proxy Functionality**: The application forwards HTTP requests to a specified target URL and returns the response (status code, headers, and content) back to the client.

## Endpoints

### `GET /proxy`

This is the main endpoint of the application, which acts as a proxy for forwarding requests to external URLs.

#### Query Parameters:
- `url`: The target URL that you want to proxy the request to (e.g., `http://example.com/api/data`).

#### Headers:
- **Authorization** (optional): If the `Authorization` header is present in the incoming request, it will be forwarded to the target URL.

#### Response:
- The proxy will forward the content of the response from the target URL, along with the original content type and status code.

## Example Request

```bash
GET /proxy?url=http://example.com/api/data
Authorization: Bearer <Your-Token>
```

## Example Response

```http
HTTP/1.1 200 OK
Content-Type: application/json
Content-Length: 1234
```

The content of the response body will be the same as the content returned by the target URL.

## Usage

1. **Clone the Repository**:
   - Clone this repository to your local machine.

2. **Build the Application**:
   - Run the following command to build the application:
   
   ```bash
   dotnet build
   ```

3. **Run the Application**:
   - Start the application using:
   
   ```bash
   dotnet run
   ```

4. **Access the API**:
   - The application will start on `http://localhost:5000` by default.
   - You can test the `/proxy` endpoint by providing the `url` query parameter and optionally the `Authorization` header.

## Docker Setup

To deploy this application using Docker, use the following `Dockerfile`:

```dockerfile
# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Use the .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

1. Build the Docker image:

```bash
docker build -t yourapp .
```

2. Run the container:

```bash
docker run -d -p 5000:5000 yourapp
```

Now, you can access the proxy service on `http://localhost:5000`.

## Pre-built Docker Image

You can also use the pre-built Docker image from Docker Hub. The image is available here:

[**Docker Hub - lemmiwinks**](https://hub.docker.com/repository/docker/marcokreeft/lemmiwinks/general)

To pull the image and run it:

```bash
docker pull marcokreeft/lemmiwinks
docker run -d -p 5000:5000 marcokreeft/lemmiwinks
```

Now, you can access the proxy service on `http://localhost:5000`.

---

## Security Note

- **SSL Validation**: The application bypasses SSL validation for development environments. This should never be used in production environments where security is a concern.
  
