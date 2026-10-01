# ETAPA 1: Compilación
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj respetando las rutas del proyecto
COPY ["src/GestionSolicitudes.Api/GestionSolicitudes.Api.csproj", "src/GestionSolicitudes.Api/"]
COPY ["src/GestionSolicitudes.Application/GestionSolicitudes.Application.csproj", "src/GestionSolicitudes.Application/"]
COPY ["src/GestionSolicitudes.Domain/GestionSolicitudes.Domain.csproj", "src/GestionSolicitudes.Domain/"]
COPY ["src/GestionSolicitudes.Infrastructure/GestionSolicitudes.Infrastructure.csproj", "src/GestionSolicitudes.Infrastructure/"]
COPY ["GestionSolicitudes.Client/GestionSolicitudes.Client.csproj", "GestionSolicitudes.Client/"]

# Restaurar paquetes
RUN dotnet restore "src/GestionSolicitudes.Api/GestionSolicitudes.Api.csproj"

# Copiar el resto del código y compilar la API
COPY . .
WORKDIR "/src/src/GestionSolicitudes.Api"
RUN dotnet publish "GestionSolicitudes.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Runtime final
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestionSolicitudes.Api.dll"]