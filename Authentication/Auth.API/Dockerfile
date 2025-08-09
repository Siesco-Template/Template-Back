FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5032

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Authentication/Auth.API/Auth.API.csproj"
RUN dotnet build "Authentication/Auth.API/Auth.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Authentication/Auth.API/Auth.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet","Auth.API.dll"]