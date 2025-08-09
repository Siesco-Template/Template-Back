FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5032
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR "src/Authentication/Auth.API"
RUN dotnet restore "Auth.API.csproj"
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]