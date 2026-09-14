# Notes API — C# / ASP.NET Core / PostgreSQL

Contoh project belajar backend menggunakan **C# + .NET 10 + ASP.NET Core + PostgreSQL + Docker Compose**.

Project ini sengaja dibuat dengan **Clean Architecture yang sederhana dan idiomatic .NET**, supaya konsepnya mudah dibandingkan dengan backend Go.

> **Catatan keamanan:** Basic Authentication di project ini hanya untuk belajar. Basic Auth mengirim username/password dalam bentuk Base64, bukan encryption. Gunakan HTTPS/TLS dan mekanisme identity provider/password hashing yang proper untuk production.

## 1. Stack

- C# / .NET 10
- ASP.NET Core Web API
- Controller-based API
- PostgreSQL 17
- Entity Framework Core 10
- Npgsql
- Docker + Docker Compose
- Basic Authentication
- Environment variables
- Clean Architecture
- xUnit untuk unit test Domain

## 2. Arsitektur

```text
src/
├── Notes.Api/
│   ├── Authentication/       # Basic Auth handler dan options
│   ├── Contracts/            # HTTP request DTO
│   ├── Controllers/          # HTTP layer
│   ├── Middleware/           # Global exception handling
│   ├── Extensions/           # Dependency Injection registration
│   └── Program.cs
│
├── Notes.Application/
│   ├── Abstractions/         # Interface repository
│   └── Notes/                # Use case/service + DTO aplikasi
│
├── Notes.Domain/
│   └── Entities/             # Entity + business rules
│
└── Notes.Infrastructure/
    ├── Persistence/          # EF Core DbContext
    └── Repositories/         # Implementasi repository PostgreSQL
```

Dependency direction:

```text
             Notes.Api
                 │
        ┌────────┴─────────┐
        ▼                  ▼
 Notes.Application   Notes.Infrastructure
        │                  │
        ▼                  ▼
    Notes.Domain      PostgreSQL / EF Core
```

Domain tidak mengetahui ASP.NET Core atau PostgreSQL.

## 3. Konsep Clean Architecture yang dipakai

### Domain

Berisi business rule murni.

```csharp
var note = Note.Create(title, content);
note.Update(title, content);
```

Domain tidak melakukan HTTP atau SQL.

### Application

Mengorkestrasi use case:

```text
HTTP request
    ↓
Controller
    ↓
NoteService
    ↓
INoteRepository
```

Application hanya mengenal interface repository.

### Infrastructure

Implementasi detail teknis:

```text
INoteRepository
      ↓
NoteRepository
      ↓
EF Core
      ↓
PostgreSQL
```

### API

Bertanggung jawab atas:

- HTTP
- authentication
- controller
- request validation
- response status code
- middleware

## 4. Environment Variables

Jangan commit `.env` asli.

Buat dari template:

```bash
cp .env.example .env
```

Contoh:

```env
POSTGRES_DB=notesdb
POSTGRES_USER=notes
POSTGRES_PASSWORD=notes_dev_password
POSTGRES_PORT=5432

BASIC_AUTH_USERNAME=admin
BASIC_AUTH_PASSWORD=admin123

API_PORT=8080
ASPNETCORE_ENVIRONMENT=Development
```

ASP.NET Core menggunakan konvensi `__` untuk nested configuration.

Contoh:

```text
BasicAuth__Username
```

akan dibaca sebagai:

```text
BasicAuth:Username
```

Sedangkan:

```text
ConnectionStrings__Default
```

menjadi:

```text
ConnectionStrings:Default
```

## 5. Menjalankan dengan Docker Compose

### Setup environment

```bash
cp .env.example .env
```

### Start

```bash
make compose-up
```

Cek container:

```bash
docker compose ps
```

Cek log:

```bash
make compose-logs
```

API tersedia di:

```text
http://localhost:8080
```

Health check:

```bash
curl http://localhost:8080/health
```

Response:

```json
{
  "status": "ok"
}
```

## 6. Basic Authentication

Credential diambil dari environment:

```text
BASIC_AUTH_USERNAME
BASIC_AUTH_PASSWORD
```

Test endpoint:

```bash
curl -u admin:admin123 http://localhost:8080/api/v1/notes
```

Tanpa credential:

```bash
curl -i http://localhost:8080/api/v1/notes
```

Akan mendapat `401 Unauthorized`.

## 7. CRUD API

Base URL:

```text
/api/v1/notes
```

Semua endpoint Notes membutuhkan Basic Authentication.

### Create

```bash
curl -u admin:admin123 \
  -X POST http://localhost:8080/api/v1/notes \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Belajar ASP.NET Core",
    "content": "Memahami Clean Architecture di .NET."
  }'
```

### Get all

```bash
curl -u admin:admin123 \
  http://localhost:8080/api/v1/notes
```

### Get by ID

```bash
curl -u admin:admin123 \
  http://localhost:8080/api/v1/notes/{id}
```

### Update

```bash
curl -u admin:admin123 \
  -X PUT http://localhost:8080/api/v1/notes/{id} \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Belajar ASP.NET Core — Update",
    "content": "Sudah memahami controller dan DI."
  }'
```

### Delete

```bash
curl -u admin:admin123 \
  -X DELETE http://localhost:8080/api/v1/notes/{id}
```

Response sukses delete adalah `204 No Content`.

## 8. HTTP Status Code

| Operasi | Success | Failure |
|---|---:|---:|
| GET all | 200 | 401 |
| GET by ID | 200 | 401 / 404 |
| POST | 201 | 400 / 401 |
| PUT | 200 | 400 / 401 / 404 |
| DELETE | 204 | 401 / 404 |

## 9. Database

Docker Compose menjalankan PostgreSQL:

```text
notes-api container
       │
       │ TCP 5432
       ▼
postgres container
       │
       ▼
notesdb
```

Entity Framework Core membuat schema ketika API pertama kali start melalui `EnsureCreatedAsync()`.

**Untuk production**, sebaiknya gunakan EF Core migrations yang dijalankan sebagai deployment step terkontrol, bukan `EnsureCreatedAsync()`.

## 10. Makefile

```bash
make help
```

Perintah utama:

```bash
make restore
make build
make test
make run
make compose-up
make compose-down
make compose-logs
make clean
```

## 11. Local development tanpa Docker API

Kalau PostgreSQL tersedia di host, konfigurasi:

```bash
export ConnectionStrings__Default='Host=localhost;Port=5432;Database=notesdb;Username=notes;Password=notes_dev_password'
export BasicAuth__Username='admin'
export BasicAuth__Password='admin123'
```

Kemudian:

```bash
make run
```

## 12. Perbandingan dengan Go

```text
Go                         ASP.NET Core
──────────────────────────────────────────────
Go                         C#
go.mod                     .csproj
Gin/Chi/Fiber              ASP.NET Core
handler                    Controller/Endpoint
middleware                 Middleware
interface                  interface
struct                     class/record
GORM                       EF Core
database/sql               EF Core / ADO.NET
context.Context             CancellationToken
wire dependencies          built-in DI container
Docker                     Docker
PostgreSQL                 PostgreSQL
```

## 13. Production Improvements Implemented

Proyek ini telah secara lengkap mengimplementasikan 15 item peningkatan standar enterprise *production-ready*:

- [x] **1. EF Core migrations**: Menggunakan `AppDbContextFactory` dan `Database.MigrateAsync()` otomatis pada startup aplikasi.
- [x] **2. Password Hashing**: Menggunakan hashing terenskripsi BCrypt (`BCrypt.Net-Next`) di `PasswordHasher.cs`.
- [x] **3. JWT / OAuth2**: Implementasi JWT Bearer token authentication dengan `IJwtTokenGenerator` dan `AddJwtAuthentication`.
- [x] **4. Refresh Token**: Dukungan refresh token persisten dengan rotasi token di `AuthService.cs` (`POST /api/v1/auth/refresh`).
- [x] **5. Pagination & Filtering**: Filter pencarian judul/konten (`searchKeyword`) dan pengurutan dinamis (`sortBy`, `sortOrder`).
- [x] **6. Optimistic Concurrency**: Penggunaan PostgreSQL `xmin` concurrency token untuk mendeteksi konflik edit bersamaan (`409 Conflict`).
- [x] **7. Structured Logging**: Integrasi Serilog terstruktur berbasis JSON dan context tracking di `Program.cs`.
- [x] **8. OpenTelemetry**: Telemetri standar OpenTelemetry untuk tracing & metrics ASP.NET Core & EF Core.
- [x] **9. Integration Test**: Pengujian integrasi otomatis menggunakan `WebApplicationFactory<Program>` di `tests/Notes.IntegrationTests`.
- [x] **10. Docker Image Hardening**: Runtime Dockerfile aman menggunakan non-root user `appuser` (UID 10001).
- [x] **11. Health Checks**: Endpoint `/health` dengan verifikasi koneksi database EF Core (`AddDbContextCheck<AppDbContext>`).
- [x] **12. CI/CD**: Workflow GitHub Actions otomatis di `.github/workflows/ci.yml`.
- [x] **13. Secret Management**: Strongly typed configuration (`JwtOptions`, `BasicAuthOptions`) yang terisolasi aman.
- [x] **14. API Versioning**: Versi API terstandar menggunakan `Asp.Versioning.Http` dan `Asp.Versioning.Mvc`.
- [x] **15. Standardized ProblemDetails ErrorResponse**: Envelope kesalahan terpadu berstandar RFC 7807 dengan `traceId` correlation.

