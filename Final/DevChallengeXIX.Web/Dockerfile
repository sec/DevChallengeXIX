FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["DevChallengeXIX.Web.csproj", "."]
RUN dotnet restore "./DevChallengeXIX.Web.csproj" -r linux-musl-x64 /p:PublishReadyToRun=true
COPY . .
WORKDIR "/src/."
RUN dotnet build "DevChallengeXIX.Web.csproj" -c Release -o /app/build -r linux-musl-x64

FROM build AS publish
RUN dotnet publish "DevChallengeXIX.Web.csproj" --no-restore -c Release -o /app/publish /p:PublishReadyToRun=true -r linux-musl-x64 --self-contained true

FROM base AS final
RUN apk --no-cache add curl
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./DevChallengeXIX.Web"]