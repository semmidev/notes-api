SHELL := /bin/bash

.PHONY: help restore build test run compose-up compose-down compose-logs migrate-clean clean

help:
	@echo "Perintah yang tersedia:"
	@echo "  make restore       Restore dependency NuGet"
	@echo "  make build         Build aplikasi"
	@echo "  make test          Jalankan unit test"
	@echo "  make run           Jalankan API secara lokal"
	@echo "  make compose-up    Build + jalankan API dan PostgreSQL"
	@echo "  make compose-down  Hentikan container"
	@echo "  make compose-logs  Lihat log container"
	@echo "  make clean         Bersihkan hasil build"

restore:
	dotnet restore src/Notes.Api/Notes.Api.csproj

build:
	dotnet build src/Notes.Api/Notes.Api.csproj

test:
	dotnet test tests/Notes.Application.Tests/Notes.Application.Tests.csproj

run:
	dotnet run --project src/Notes.Api/Notes.Api.csproj

compose-up:
	docker compose up --build -d

compose-down:
	docker compose down

compose-logs:
	docker compose logs -f api

clean:
	dotnet clean src/Notes.Api/Notes.Api.csproj
