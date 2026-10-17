FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY Krono.sln ./
COPY src/Krono.Domain/*.csproj ./src/Krono.Domain/
COPY src/Krono.Application/*.csproj ./src/Krono.Application/
COPY src/Krono.Infrastructure/*.csproj ./src/Krono.Infrastructure/
COPY src/Krono.API/*.csproj ./src/Krono.API/
COPY tests/Krono.Tests/*.csproj ./tests/Krono.Tests/

# Restore dependencies
RUN dotnet restore Krono.sln

# Copy entire source
COPY src/ ./src/
COPY tests/ ./tests/

# Build and publish
RUN dotnet publish src/Krono.API/Krono.API.csproj -c Release -o /out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Krono.API.dll"]
