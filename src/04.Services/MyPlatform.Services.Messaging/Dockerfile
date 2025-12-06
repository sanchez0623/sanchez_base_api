FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/04.Services/MyPlatform.Services.Messaging/MyPlatform.Services.Messaging.csproj", "src/04.Services/MyPlatform.Services.Messaging/"]
COPY ["src/01.Shared/MyPlatform.Shared.Contracts/MyPlatform.Shared.Contracts.csproj", "src/01.Shared/MyPlatform.Shared.Contracts/"]
COPY ["src/02.SDK/MyPlatform.SDK.Authentication/MyPlatform.SDK.Authentication.csproj", "src/02.SDK/MyPlatform.SDK.Authentication/"]
COPY ["src/02.SDK/MyPlatform.SDK.Authorization/MyPlatform.SDK.Authorization.csproj", "src/02.SDK/MyPlatform.SDK.Authorization/"]
COPY ["src/02.SDK/MyPlatform.SDK.EventBus/MyPlatform.SDK.EventBus.csproj", "src/02.SDK/MyPlatform.SDK.EventBus/"]
COPY ["src/02.SDK/MyPlatform.SDK.MultiTenancy/MyPlatform.SDK.MultiTenancy.csproj", "src/02.SDK/MyPlatform.SDK.MultiTenancy/"]
COPY ["src/02.SDK/MyPlatform.SDK.Observability/MyPlatform.SDK.Observability.csproj", "src/02.SDK/MyPlatform.SDK.Observability/"]
COPY ["src/03.Infrastructure/MyPlatform.Infrastructure.EFCore/MyPlatform.Infrastructure.EFCore.csproj", "src/03.Infrastructure/MyPlatform.Infrastructure.EFCore/"]

RUN dotnet restore "./src/04.Services/MyPlatform.Services.Messaging/MyPlatform.Services.Messaging.csproj"

COPY . .
WORKDIR "/src/src/04.Services/MyPlatform.Services.Messaging"
RUN dotnet build "./MyPlatform.Services.Messaging.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyPlatform.Services.Messaging.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyPlatform.Services.Messaging.dll"]
