FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5100
ENV ASPNETCORE_URLS=http://+:5100

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TabibLens.Web/TabibLens.Web.csproj", "TabibLens.Web/"]
RUN dotnet restore "./TabibLens.Web/TabibLens.Web.csproj"
COPY . .
WORKDIR "/src/TabibLens.Web"
RUN dotnet build "./TabibLens.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TabibLens.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TabibLens.Web.dll"]
