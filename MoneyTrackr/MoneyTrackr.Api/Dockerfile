FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY MoneyTrackr.Api.csproj .
RUN dotnet restore MoneyTrackr.Api.csproj
COPY . .
RUN dotnet build MoneyTrackr.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish MoneyTrackr.Api.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf