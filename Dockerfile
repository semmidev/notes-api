# Stage 1: compile dan publish aplikasi menggunakan .NET SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Notes.slnx Directory.Build.props ./
COPY src/Notes.Domain/Notes.Domain.csproj src/Notes.Domain/
COPY src/Notes.Application/Notes.Application.csproj src/Notes.Application/
COPY src/Notes.Infrastructure/Notes.Infrastructure.csproj src/Notes.Infrastructure/
COPY src/Notes.Api/Notes.Api.csproj src/Notes.Api/

RUN dotnet restore src/Notes.Api/Notes.Api.csproj

COPY src ./src
RUN dotnet publish src/Notes.Api/Notes.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime image yang lebih kecil dan hardened.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

USER app

COPY --from=build --chown=app:app /app/publish .

ENTRYPOINT ["dotnet", "Notes.Api.dll"]
