FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/IdentityServer
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

COPY ../aspnetcore-dev-cert.pfx /usr/local/share/ca-certificates/aspnetcore-dev-cert.crt
RUN update-ca-certificates

EXPOSE 80 443
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENTRYPOINT ["dotnet", "IdentityServer.dll"]