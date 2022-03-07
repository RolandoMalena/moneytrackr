ARG ENVIRONMENT
FROM moneytrackr_common-${ENVIRONMENT} AS common

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
COPY --from=common . .
WORKDIR /MoneyTrackr/src
COPY MoneyTrackr.Api.csproj .
RUN dotnet restore MoneyTrackr.Api.csproj
COPY . .
RUN dotnet build MoneyTrackr.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish MoneyTrackr.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoneyTrackr.Api.dll"]