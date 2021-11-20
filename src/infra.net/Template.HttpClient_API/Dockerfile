#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Template.HttpClient_API/Template.HttpClient_API.csproj", "Template.HttpClient_API/"]
COPY ["Infra.Net.LogManager/Infra.Net.LogManager.csproj", "Infra.Net.LogManager/"]
COPY ["Infra.Net.Extensions/Infra.Net.Extensions.csproj", "Infra.Net.Extensions/"]
COPY ["Infra.Net.CacheManager.Http/Infra.Net.CacheManager.Http.csproj", "Infra.Net.CacheManager.Http/"]
COPY ["Infra.Net.CacheManager/Infra.Net.CacheManager.csproj", "Infra.Net.CacheManager/"]
COPY ["Infra.Net.HttpClientManager/Infra.Net.HttpClientManager.csproj", "Infra.Net.HttpClientManager/"]
COPY ["Infra.Net.Helpers/Infra.Net.Helpers.csproj", "Infra.Net.Helpers/"]
COPY ["Infra.Net.HttpClientManager.Compatibility/Infra.Net.HttpClientManager.Compatibility.csproj", "Infra.Net.HttpClientManager.Compatibility/"]
COPY ["Infra.Net.LogManager.WebExtensions/Infra.Net.LogManager.WebExtensions.csproj", "Infra.Net.LogManager.WebExtensions/"]
RUN dotnet restore "Template.HttpClient_API/Template.HttpClient_API.csproj"
COPY . .
WORKDIR "/src/Template.HttpClient_API"
RUN dotnet build "Template.HttpClient_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.HttpClient_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.HttpClient_API.dll"]
