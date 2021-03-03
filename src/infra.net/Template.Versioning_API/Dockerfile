#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Template.Versioning_API/Template.Versioning_API.csproj", "Template.Versioning_API/"]
RUN dotnet restore "Template.Versioning_API/Template.Versioning_API.csproj"
COPY . .
WORKDIR "/src/Template.Versioning_API"
RUN dotnet build "Template.Versioning_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.Versioning_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.Versioning_API.dll"]
