# Multi-stage build for ConnectionHolderWorker
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy NuGet.config first for package sources
COPY ["Chat/NuGet.config", "./"]

# Copy solution and project files
COPY ["Chat/Chat.sln", "./"]
COPY ["Chat/Core/Application/Application.csproj", "Chat/Core/Application/"]
COPY ["Chat/Core/Domain/Domain.csproj", "Chat/Core/Domain/"]
COPY ["Chat/External/Infrastructure/Infrastructure.csproj", "Chat/External/Infrastructure/"]
COPY ["Chat/External/Workers/ConnectionHolderWorker/ConnectionHolderWorker.csproj", "Chat/External/Workers/ConnectionHolderWorker/"]

# Restore dependencies
RUN dotnet restore "Chat/External/Workers/ConnectionHolderWorker/ConnectionHolderWorker.csproj"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "Chat/External/Workers/ConnectionHolderWorker/ConnectionHolderWorker.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Chat/External/Workers/ConnectionHolderWorker/ConnectionHolderWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=publish --chown=appuser:appuser /app/publish .

# Expose port (if needed for health checks)
EXPOSE 11001

# Set entry point
ENTRYPOINT ["dotnet", "ConnectionHolderWorker.dll"] 