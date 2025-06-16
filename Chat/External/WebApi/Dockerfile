FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 11000
EXPOSE 11001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Chat/External/WebApi/WebApi.csproj", "Chat/External/WebApi/"]
COPY ["Chat/Core/Domain/Domain.csproj", "Chat/Core/Domain/"]
COPY ["Chat/Core/Application/Application.csproj", "Chat/Core/Application/"]
COPY ["Chat/External/Infrastructure/Infrastructure.csproj", "Chat/External/Infrastructure/"]
COPY ["NuGet.config", "NEVA.FriendsSystem/"]
RUN dotnet restore "Chat/External/WebApi/WebApi.csproj"
COPY . .
WORKDIR "/src/Chat/External/WebApi"
RUN dotnet build "WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
