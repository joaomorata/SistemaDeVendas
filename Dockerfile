FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TelaLoginCrud/TelaLoginCrud.csproj TelaLoginCrud/
RUN dotnet restore TelaLoginCrud/TelaLoginCrud.csproj
COPY . .
RUN dotnet publish TelaLoginCrud/TelaLoginCrud.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "TelaLoginCrud.dll"]
