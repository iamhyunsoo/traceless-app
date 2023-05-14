FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 80

# Configure the below line in docker-compose.yml instead of here.
# ENV ASPNETCORE_URLS=http://+:9990/

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build

WORKDIR /DataAccess
COPY ./DataAccess .
RUN dotnet restore "DataAccess.csproj"

WORKDIR /Server
COPY ./Server .
RUN dotnet restore "Server.csproj"
RUN dotnet build "Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Server.dll"]