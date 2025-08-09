FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5031

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Gateway/Template.Gateway/Template.Gateway.csproj"
RUN dotnet build "Gateway/Template.Gateway/Template.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway/Template.Gateway/Template.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.Gateway.dll"]