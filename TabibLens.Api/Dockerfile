FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5081
ENV ASPNETCORE_URLS=http://+:5081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TabibLens.Api/TabibLens.Api.csproj", "TabibLens.Api/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infra/Infra.csproj", "Infra/"]
RUN dotnet restore "./TabibLens.Api/TabibLens.Api.csproj"
COPY . .
WORKDIR "/src/TabibLens.Api"
RUN dotnet build "./TabibLens.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TabibLens.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TabibLens.Api.dll"]
