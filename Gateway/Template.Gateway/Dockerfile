FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5031

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/Gateway/Template.Gateway"
RUN dotnet restore "Template.Gateway.csproj"
RUN dotnet build "Template.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.Gateway.dll"]