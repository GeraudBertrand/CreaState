# SDK on the build machine's native arch, cross-compiles for TARGETARCH (no QEMU needed)
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy project file and restore
COPY ["CreaState.csproj", "./"]
RUN dotnet restore "CreaState.csproj" -a $TARGETARCH

# Copy everything else and publish
COPY . .
RUN dotnet publish "CreaState.csproj" -c Release -a $TARGETARCH --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "CreaState.dll"]
