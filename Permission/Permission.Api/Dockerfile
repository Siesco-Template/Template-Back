FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5033

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR "Permission/Permission.Api"
RUN dotnet restore "Permission.Api.csproj"
RUN dotnet build "Permission.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Permission.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Permission.Api.dll"]