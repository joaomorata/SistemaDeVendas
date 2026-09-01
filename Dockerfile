FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SistemaDeVendas/SistemaDeVendas.csproj SistemaDeVendas/
RUN dotnet restore SistemaDeVendas/SistemaDeVendas.csproj
COPY . .
RUN dotnet publish SistemaDeVendas/SistemaDeVendas.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:10000
# Containers do Render tem limite baixo de inotify; desliga o watch dos arquivos de config
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 10000
ENTRYPOINT ["dotnet", "SistemaDeVendas.dll"]
