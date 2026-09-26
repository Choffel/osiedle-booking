FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["osiedle-booking.WebApi/osiedle-booking.WebApi.csproj", "osiedle-booking.WebApi/"]
RUN dotnet restore "osiedle-booking.WebApi/osiedle-booking.WebApi.csproj"
COPY . .
WORKDIR "/src/osiedle-booking.WebApi"
RUN dotnet build "./osiedle-booking.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./osiedle-booking.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "osiedle-booking.WebApi.dll"]
