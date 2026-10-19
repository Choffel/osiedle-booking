FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["osiedle-booking.WebApi/osiedle-booking.WebApi.csproj", "osiedle-booking.WebApi/"]

COPY ["Modules/Users/Users.Application/Users.Application.csproj", "Modules/Users/Users.Application/"]
COPY ["Modules/Users/Users.Domain/Users.Domain.csproj", "Modules/Users/Users.Domain/"]
COPY ["Modules/Users/Users.Infrastructure/Users.Infrastructure.csproj", "Modules/Users/Users.Infrastructure/"]

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