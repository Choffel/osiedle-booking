FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["osiedle-booking.WebApi/osiedle-booking.WebApi.csproj", "osiedle-booking.WebApi/"]

COPY ["Modules/Users/Users.Appliation/Users.Appliation.csproj", "Modules/Users/Users.Appliation/"]
COPY ["Modules/Users/Users.Domain/Users.Domain.csproj", "Modules/Users/Users.Domain/"]
COPY ["Modules/Users/Users.Infrastructure/Users.Infrastructure.csproj", "Modules/Users/Users.Infrastructure/"]

COPY ["Modules/Business/Business.Application/Business.Application.csproj", "Modules/Business/Business.Application/"]
COPY ["Modules/Business/Business.Domain/Business.Domain.csproj", "Modules/Business/Business.Domain/"]
COPY ["Modules/Business/Business.Infrastructure/Business.Infrastructure.csproj", "Modules/Business/Business.Infrastructure/"]

COPY ["BuildingBlocks/osiedle-booking.BuildingBlocks/osiedle-booking.BuildingBlocks.csproj", "BuildingBlocks/osiedle-booking.BuildingBlocks/"]

RUN dotnet restore "osiedle-booking.WebApi/osiedle-booking.WebApi.csproj"

COPY . .

WORKDIR "/src/osiedle-booking.WebApi"
RUN dotnet build "osiedle-booking.WebApi.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "osiedle-booking.WebApi.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "osiedle-booking.WebApi.dll"]