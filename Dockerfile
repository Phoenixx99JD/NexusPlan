FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/NexusPlan.Domain/NexusPlan.Domain.csproj src/NexusPlan.Domain/
COPY src/NexusPlan.Application/NexusPlan.Application.csproj src/NexusPlan.Application/
COPY src/NexusPlan.Infrastructure/NexusPlan.Infrastructure.csproj src/NexusPlan.Infrastructure/
COPY src/NexusPlan.Api/NexusPlan.Api.csproj src/NexusPlan.Api/
RUN dotnet restore src/NexusPlan.Api/NexusPlan.Api.csproj
COPY src/ src/
RUN dotnet publish src/NexusPlan.Api/NexusPlan.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusPlan.Api.dll"]
