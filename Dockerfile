# Use the official .NET image as a base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Program.cs", "./"]
COPY ["Lemmiwinks.csproj", "./"]
RUN dotnet restore ./Lemmiwinks.csproj
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lemmiwinks.dll"]
