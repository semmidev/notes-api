# Panduan Lengkap .NET & C# Notes API (Belajar dari Nol)

Selamat datang! Dokumen ini dibuat khusus untuk membantu pengembang yang baru mengenal C# dan .NET agar dapat memahami konsep dasar, arsitektur Clean Architecture, cara kerja setiap file, serta langkah-langkah membuat ulang (*reproduce*) proyek ini dari terminal dari nol.

---

## Daftar Isi
1. [Konsep Dasar .NET & C#](#1-konsep-dasar-net--c)
2. [Arsitektur Proyek (Clean Architecture)](#2-arsitektur-proyek-clean-architecture)
3. [Fitur & Bahasa C# yang Digunakan di Proyek Ini](#3-fitur--bahasa-c-yang-digunakan-di-proyek-ini)
4. [Penjelasan Detail Struktur File & Folder](#4-penjelasan-detail-struktur-file--folder)
5. [Alur Eksekusi Request HTTP (Request Lifecycle)](#5-alur-eksekusi-request-http-request-lifecycle)
6. [Panduan Membuat Ulang Proyek Ini dari Nol (Step-by-Step CLI)](#6-panduan-membuat-ulang-proyek-ini-dari-nol-step-by-step-cli)
7. [Cheat Sheet Perintah CLI `dotnet`](#7-cheat-sheet-perintah-cli-dotnet)
8. [Manajeman Transaksi Database (`IUnitOfWork`) & Raw SQL Query Kompleks](#8-manajeman-transaksi-database-iunitofwork--raw-sql-query-kompleks)
9. [EF Core Migrations: Konsep & Best Practices](#9-ef-core-migrations-konsep--best-practices)
10. [Tips & Langkah Selanjutnya untuk Production](#10-tips--langkah-selanjutnya-untuk-production)

---

## 1. Konsep Dasar .NET & C#

Sebelum masuk ke kode, berikut adalah analogi dan konsep kunci yang perlu dipahami:

* **C# (C-Sharp)**: Bahasa pemrograman berorientasi objek (OOP), *strongly typed*, dan *type-safe* yang dikembangkan oleh Microsoft.
* **.NET (sebelumnya .NET Core)**: *Developer platform* / runtime lintas platform (Windows, macOS, Linux) untuk menjalankan aplikasi C#.
* **CLR (Common Language Runtime)**: "Engine" atau mesin virtual di dalam .NET yang meng-execute kode C# (kompilasi dari kode C# menjadi IL / Intermediate Language, lalu di-JIT / *Just-In-Time compile* ke bahasa mesin).
* **SDK vs Runtime**:
  * **SDK (.NET SDK)**: Alat lengkap untuk *developer* (compiler `csc`, CLI `dotnet`, generator proyek, analyzer).
  * **Runtime**: Komponen minimal hanya untuk *menjalankan* aplikasi yang sudah di-build (contohnya di container production).
* **NuGet**: Package manager resmi untuk .NET (mirip `npm` di Node.js, `pip` di Python, atau `go modules` di Go).

---

## 2. Arsitektur Proyek (Clean Architecture)

Proyek ini menerapkan **Clean Architecture** (juga dikenal sebagai *Onion Architecture* atau *Hexagonal Architecture*). Prinsip utamanya adalah **Dependency Rule**: *Ketergantungan kode hanya boleh mengarah ke dalam (ke arah Domain).*

```text
               ┌────────────────────────┐
               │       Notes.Api        │  (HTTP, Controllers, Auth, Middleware)
               └───┬────────────────┬───┘
                   │                │
                   ▼                ▼
┌──────────────────────┐   ┌──────────────────────────┐
│  Notes.Application   │   │   Notes.Infrastructure   │ (EF Core, PostgreSQL, DB Context)
└──────────┬───────────┘   └────────────┬─────────────┘
           │                            │
           ▼                            │
┌──────────────────────┐                │
│     Notes.Domain     │◄───────────────┘
└──────────────────────┘ (Business Rules & Entities Murni)
```

### Penjelasan Layer:

1. **[Notes.Domain](src/Notes.Domain)** (*Core Paling Dalam*):
   * Berisi entitas bisnis murni (`Note.cs`), aturan validasi bisnis, dan *value objects*.
   * **Bebas dari dependensi luar**: Tidak boleh mengimpor EF Core, ASP.NET, HTTP, atau database.
2. **[Notes.Application](src/Notes.Application)** (*Use Cases / Business Logic*):
   * Memuat logika aplikasi (`NoteService.cs`), DTO (*Data Transfer Object*), dan antarmuka (*interface*) repository (`INoteRepository.cs`).
   * Hanya bergantung pada **Notes.Domain**.
3. **[Notes.Infrastructure](src/Notes.Infrastructure)** (*Technical Details*):
   * Mengimplementasikan interface dari layer Application.
   * Berisi Entity Framework Core (EF Core), database `AppDbContext`, dan `NoteRepository`.
   * Bergantung pada **Notes.Application** dan **Notes.Domain**.
4. **[Notes.Api](src/Notes.Api)** (*Entry Point / Web Framework*):
   * Berisi `Program.cs`, Controller REST API, Middleware, HTTP DTO Request/Response, dan autentikasi.
   * Menghubungkan semua dependensi via *Dependency Injection* (DI).

---

## 3. Fitur & Bahasa C# yang Digunakan di Proyek Ini

Proyek ini menggunakan fitur-fitur C# modern (C# 12 / C# 13 di .NET 10). Berikut adalah sintaksis penting yang digunakan:

### A. Primary Constructors
Di C# 12+, Anda bisa mendeklarasikan parameter constructor langsung di nama class:

```csharp
// Menggunakan Primary Constructor (Modern C#):
public sealed class NoteService(INoteRepository repository)
{
    // 'repository' langsung tersedia di seluruh class!
}
```

*Dibandingkan dengan C# versi lama:*
```csharp
// Cara Tradisional C#:
public class NoteService
{
    private readonly INoteRepository _repository;
    public NoteService(INoteRepository repository)
    {
        _repository = repository;
    }
}
```

### B. Records (`record`)
Digunakan untuk data yang tidak diubah (*immutable data holder*), seperti DTO dan Request/Response.

```csharp
public sealed record CreateNoteCommand(string Title, string Content);
```

* `record` secara otomatis membuatkan method `Equals()`, `GetHashCode()`, `ToString()`, dan immutability bawaan.

### C. Encapsulation & Private Setter pada Entity
Di [Note.cs](src/Notes.Domain/Entities/Note.cs), properti menggunakan `private set`:

```csharp
public Guid Id { get; private set; }
public string Title { get; private set; } = string.Empty;
```

Ini memastikan kode di luar `Note` tidak bisa sembarangan mengubah `Title` tanpa melalui method resmi seperti `note.Update(title, content)`.

### D. Nullable Reference Types (`?`)
Untuk mencegah error klasik `NullReferenceException`, C# mengaktifkan fitur nullable context (`<Nullable>enable</Nullable>`):

* `string` -> tidak boleh `null`.
* `string?` -> boleh `null`.
* `Note?` -> return value bisa jadi `null` jika data tidak ditemukan di DB.

### E. Extension Methods & Dependency Injection
Di .NET, layanan didaftarkan ke container `IServiceCollection`. Kita membuat *Extension Method* agar pendaftaran modul rapi:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<NoteService>();
        return services;
    }
}
```

* **Lifetime Dependency Injection di .NET**:
  * `AddTransient`: Instance baru dibuat *setiap kali* disuntikkan/diminta.
  * `AddScoped`: Instance yang sama dipakai dalam *satu siklus request HTTP*. (Paling umum untuk Service & DbContext).
  * `AddSingleton`: Hany dibuat *satu kali* selama aplikasi berjalan.

### F. Async / Await & `Task` / `Task<T>`
Operasi I/O (akses DB/network) bersifat asynchronous agar server tidak *blocking*:

* **`Task`** *(Tanpa `<T>`)*: Representasi operasi async yang tidak mengembalikan nilai (mirip `void`).
* **`Task<T>`** *(Dengan `<T>`)*: Representasi operasi async yang menghasilkan nilai bertipe `T` (mirip `Promise<T>` di JS / `Future<T>` di Java).
* **`async` / `await`**: Menandai method async dan menunggu hasil `Task` tanpa memblokir thread server Kestrel.

```csharp
public async Task<NoteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
{
    var note = await repository.GetByIdAsync(id, cancellationToken);
    return note is null ? null : ToResponse(note);
}
```

### G. `CancellationToken` (Graceful Cancellation)
Token yang dikirimkan oleh ASP.NET Core ke method async. Jika client membatalkan HTTP request (misalnya menutup tab browser), `CancellationToken` membatalkan query SQL di EF Core & PostgreSQL secara otomatis agar server tidak membuang resource secara sia-sia.

### H. `IActionResult` & `ActionResult<T>`
Di Controller, `ActionResult<T>` mengontrol status code HTTP dan mengembalikan body JSON secara *type-safe*:
* `Ok(data)` -> **200 OK**
* `CreatedAtAction(...)` -> **201 Created**
* `NoContent()` -> **204 No Content**
* `BadRequest(error)` -> **400 Bad Request**
* `NotFound(error)` -> **404 Not Found**

### I. Standarisasi Response Envelope & Error Handling

Proyek ini membungkus seluruh response HTTP menggunakan skema terstandar:

1. **`ApiResponse<T>` (Single Item Response)**:
   Membungkus hasil item tunggal dengan properti `success`, `message`, `data`, dan `statusCode`.

2. **`PagedResponse<T>` (Paginated List Response)**:
   Membungkus daftar data berhalaman dengan metadata pagination di dalam sub-object `paging`:
   ```json
   {
     "success": true,
     "message": "Sukses mengambil data",
     "data": [ ... ],
     "paging": {
       "pageNumber": 1,
       "pageSize": 10,
       "totalPages": 2,
       "totalRecords": 19,
       "hasPreviousPage": false,
       "hasNextPage": true
     },
     "statusCode": 200
   }
   ```

3. **`ErrorResponse` (Machine-Readable Error Standard)**:
   Seluruh error (Domain Validation, Model Binding, Resource Not Found, Server Error) mengembalikan struktur terpadu:
   ```json
   {
     "success": false,
     "statusCode": 400,
     "errorCode": "INVALID_INPUT",
     "message": "Format input atau validasi data tidak valid.",
     "errors": [
       { "field": "Title", "message": "The Title field is required." }
     ],
     "timestamp": "2026-09-14T10:45:00.123Z",
     "traceId": "0HNOI2ALN6VVF:00000001"
   }
   ```

---

## 4. Penjelasan Detail Struktur File & Folder

Berikut adalah tur komprehensif dari setiap file dalam proyek:

```text
notes-api/
├── Directory.Build.props              # Konfigurasi compiler global untuk seluruh proyek
├── Dockerfile                         # Multi-stage Docker build
├── Makefile                           # Shortcut command (make build, make run, dll)
├── Notes.slnx                         # Solution file format XML (.NET 9/10)
├── docker-compose.yml                 # Konfigurasi container PostgreSQL & API
├── playground.http                    # File pengujian REST API (VS Code REST Client / Rider)
├── src/
│   ├── Notes.Domain/                  # Core Business Domain
│   │   ├── Entities/
│   │   │   └── Note.cs                # Aggregate Root / Entitas Note + Validasi
│   │   └── Notes.Domain.csproj
│   │
│   ├── Notes.Application/             # Business Logic / Use Cases
│   │   ├── Abstractions/
│   │   │   └── INoteRepository.cs     # Interface kontrak repository
│   │   ├── Common/
│   │   │   └── Models/
│   │   │       ├── ApiResponse.cs     # Single Item Response Envelope
│   │   │       ├── PagedResponse.cs   # Paginated List Response Envelope
│   │   │       └── PaginationParams.cs# Parameter Pagination (pageNumber & pageSize)
│   │   ├── Notes/
│   │   │   ├── NoteDtos.cs            # DTO Command & Response
│   │   │   └── NoteService.cs         # Orchestrator Use Case
│   │   └── Notes.Application.csproj
│   │
│   ├── Notes.Infrastructure/          # Implementation (EF Core & DB)
│   │   ├── Persistence/
│   │   │   └── AppDbContext.cs        # EF Core DbContext mapping ke PostgreSQL
│   │   ├── Repositories/
│   │   │   └── NoteRepository.cs     # Implementasi query SQL via EF Core
│   │   ├── DependencyInjection.cs     # Pendaftaran DbContext & Repository ke DI
│   │   └── Notes.Infrastructure.csproj
│   │
│   └── Notes.Api/                     # Presentation Layer (REST API)
│       ├── Authentication/
│       │   ├── BasicAuthOptions.cs           # Class pembaca config username/password
│       │   └── BasicAuthenticationHandler.cs # Custom Basic Auth Handler
│       ├── Contracts/
│       │   └── NoteRequests.cs               # HTTP Request DTO + Data Annotations
│       ├── Controllers/
│       │   └── NotesController.cs            # REST Controller (/api/v1/notes)
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs# DI Helper
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs# Centralized Error/Exception Handler
│       ├── Program.cs                        # Entry point aplikasi & middleware pipeline
│       └── Notes.Api.csproj
│
└── tests/
    └── Notes.Application.Tests/       # Unit Test (xUnit)
        ├── NoteTests.cs               # Unit test untuk domain entity
        └── Notes.Application.Tests.csproj
```

### Rincian Isi File Kunci:

1. **[Note.cs](src/Notes.Domain/Entities/Note.cs)**:
   * Memiliki factory method `Note.Create(title, content)` dan method `Update(title, content)`.
   * Melakukan validasi internal (misal: title tidak boleh kosong dan max 200 karakter).
2. **[INoteRepository.cs](src/Notes.Application/Abstractions/INoteRepository.cs)**:
   * Menyediakan *abstraction* operasi data (`GetAllAsync`, `GetByIdAsync`, `AddAsync`, `Remove`, `SaveChangesAsync`).
3. **[NoteService.cs](src/Notes.Application/Notes/NoteService.cs)**:
   * Memanggil `Note.Create(...)`, menyimpannya via `INoteRepository`, dan mengubah entitas menjadi `NoteResponse`.
4. **[AppDbContext.cs](src/Notes.Infrastructure/Persistence/AppDbContext.cs)**:
   * Class EF Core yang memetakan entitas C# ke tabel PostgreSQL (`notes`). Menentukan primary key (`id`), nama kolom, tipe data, dan index.
5. **[NoteRepository.cs](src/Notes.Infrastructure/Repositories/NoteRepository.cs)**:
   * Menggunakan `.AsNoTracking()` pada `GetAllAsync()` untuk mempercepat query *read-only* tanpa lacak perubahan memori EF Core.
6. **[Program.cs](src/Notes.Api/Program.cs)**:
   * Mengkonfigurasi web host, membaca konfigurasi dari environment variables, menyusun middleware HTTP pipeline (`ExceptionHandlingMiddleware` -> `UseAuthentication` -> `UseAuthorization` -> `MapControllers`), serta menjalankan `EnsureCreatedAsync()` saat awal start.
7. **[ExceptionHandlingMiddleware.cs](src/Notes.Api/Middleware/ExceptionHandlingMiddleware.cs)**:
   * Menangkap `ArgumentException` dari domain dan mengubahnya menjadi HTTP `400 Bad Request` berformat JSON standard (`application/problem+json`). Error tak terduga diubah menjadi HTTP `500 Internal Server Error`.

---

## 5. Alur Eksekusi Request HTTP (Request Lifecycle)

Ketika seorang client mengirimkan request `POST /api/v1/notes`:

```text
[ HTTP POST Request ]
       │
       ▼
[ ExceptionHandlingMiddleware ]  ── (Menangkap error dari layer bawah jika ada)
       │
       ▼
[ BasicAuthenticationHandler ]  ── (Verifikasi Header Authorization: Basic xxx)
       │
       ▼
[ NotesController.Create ]       ── (Validasi Data Annotations [Required, MaxLength])
       │
       ▼
[ NoteService.CreateAsync ]      ── (Orkestrasi use case)
       │
       ▼
[ Note.Create (Domain) ]         ── (Validasi aturan bisnis & pembuatan ID/timestamp)
       │
       ▼
[ NoteRepository.AddAsync ]      ── (Menambahkan objek ke EF Core DbSet)
       │
       ▼
[ AppDbContext.SaveChangesAsync ] ── (EF Core membuat & mengeksekusi SQL `INSERT INTO notes...` ke PostgreSQL)
       │
       ▼
[ Return 201 Created Response ]
```

---

## 6. Panduan Membuat Ulang Proyek Ini dari Nol (Step-by-Step CLI)

Jika Anda ingin mereproduksi proyek ini dari terminal kosong untuk latihan, ikuti langkah-langkah berikut:

### Langkah 1: Buat Direktori Utama & Solution File
```bash
mkdir notes-api
cd notes-api

# Buat solution file (.slnx di .NET 9/10 atau .sln)
dotnet new sln --name Notes
```

### Langkah 2: Buat Proyek untuk Setiap Layer
```bash
# Layer Domain (Class Library)
dotnet new classlib -o src/Notes.Domain -f net10.0

# Layer Application (Class Library)
dotnet new classlib -o src/Notes.Application -f net10.0

# Layer Infrastructure (Class Library)
dotnet new classlib -o src/Notes.Infrastructure -f net10.0

# Layer API (Web API Controller-based)
dotnet new webapi -o src/Notes.Api -f net10.0 --no-openapi

# Layer Unit Test (xUnit)
dotnet new xunit -o tests/Notes.Application.Tests -f net10.0
```

### Langkah 3: Masukkan Semua Proyek ke Solution
```bash
dotnet sln add src/Notes.Domain/Notes.Domain.csproj
dotnet sln add src/Notes.Application/Notes.Application.csproj
dotnet sln add src/Notes.Infrastructure/Notes.Infrastructure.csproj
dotnet sln add src/Notes.Api/Notes.Api.csproj
dotnet sln add tests/Notes.Application.Tests/Notes.Application.Tests.csproj
```

### Langkah 4: Hubungkan Reference Antar-Proyek (Dependency Direction)
```bash
# Application butuh Domain
dotnet add src/Notes.Application/Notes.Application.csproj reference src/Notes.Domain/Notes.Domain.csproj

# Infrastructure butuh Application (dan secara transitif Domain)
dotnet add src/Notes.Infrastructure/Notes.Infrastructure.csproj reference src/Notes.Application/Notes.Application.csproj

# API butuh Application dan Infrastructure
dotnet add src/Notes.Api/Notes.Api.csproj reference src/Notes.Application/Notes.Application.csproj
dotnet add src/Notes.Api/Notes.Api.csproj reference src/Notes.Infrastructure/Notes.Infrastructure.csproj

# Test butuh Application dan Domain
dotnet add tests/Notes.Application.Tests/Notes.Application.Tests.csproj reference src/Notes.Application/Notes.Application.csproj
```

### Langkah 5: Install Package NuGet di Infrastructure
```bash
# Package EF Core & Provider PostgreSQL (Npgsql)
dotnet add src/Notes.Infrastructure/Notes.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 10.0.0
dotnet add src/Notes.Infrastructure/Notes.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
```

### Langkah 6: Tulis Kode & Verifikasi Build
Setelah menulis kode di masing-masing file sesuai struktur, jalankan perintah berikut untuk memastikan kompilasi berhasil:

```bash
# Restore dependency
dotnet restore

# Build seluruh solution
dotnet build

# Jalankan Unit Test
dotnet test
```

---

## 7. Cheat Sheet Perintah CLI `dotnet`

| Perintah | Fungsi |
|---|---|
| `dotnet restore` | Mendownload semua dependensi NuGet yang diperlukan. |
| `dotnet build` | Kompilasi seluruh kode proyek untuk mengecek syntax/type error. |
| `dotnet run --project src/Notes.Api` | Menjalankan aplikasi Web API secara lokal. |
| `dotnet test` | Menjalankan seluruh unit test dalam solution. |
| `dotnet clean` | Membersihkan folder hasil build (`bin/` dan `obj/`). |
| `dotnet publish -c Release` | Melakukan kompilasi production-ready ke folder output. |
| `dotnet ef migrations add <NamaMigration> --project src/Notes.Infrastructure --startup-project src/Notes.Api` | Membuat file migrasi baru berdasarkan perubahan model EF Core. |
| `dotnet ef database update --project src/Notes.Infrastructure --startup-project src/Notes.Api` | Menerapkan migrasi pending ke database target. |

---

## 8. Manajeman Transaksi Database (`IUnitOfWork`) & Raw SQL Query Kompleks

### A. Database Transactions (`IUnitOfWork`)

Dalam Clean Architecture, ketika sebuah *use case* melakukan beberapa mutasi data (misalnya: membuat catatan, memperbarui counter, dan menambahkan log audit), kita harus menjamin bahwa **semua operasi berhasil bersamaan atau dibatalkan seluruhnya (*Atomicity*)**.

- **Interface Abstraction**: [IUnitOfWork.cs](src/Notes.Application/Abstractions/IUnitOfWork.cs) di layer Application.
- **Implementasi Concrete**: [UnitOfWork.cs](src/Notes.Infrastructure/Persistence/UnitOfWork.cs) di layer Infrastructure yang membungkus `IDbContextTransaction` dari EF Core.

```csharp
public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
{
    using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        await action();
        await transaction.CommitAsync(cancellationToken);
    }
    catch
    {
        await transaction.RollbackAsync(cancellationToken);
        throw;
    }
}
```

### B. Raw SQL Query Kompleks & Analitik (`Database.SqlQuery`)

Meskipun LINQ EF Core sangat andal untuk operasi CRUD harian, query kompleks seperti **analitik, laporan bulanan, window functions, CTE, atau agregasi agregat berat** lebih efisien dan jelas jika ditulis menggunakan Raw SQL murni.

Di .NET 8/9/10, EF Core menyediakan `Database.SqlQuery<T>($"""...""")` yang mendukung **String Interpolation Type-Safe** dan **otomatis terhindar dari SQL Injection** (karena string interpolated diubah menjadi `DbParameter`).

Contoh di [NoteRepository.cs](src/Notes.Infrastructure/Repositories/NoteRepository.cs):

```csharp
public async Task<NoteAnalyticsResponse> GetAnalyticsAsync(CancellationToken cancellationToken = default)
{
    var monthlyStats = await context.Database.SqlQuery<MonthlyNoteStat>($"""
        SELECT 
            TO_CHAR("CreatedAt", 'YYYY-MM') AS "Month",
            COUNT(*)::INT AS "TotalNotes",
            COALESCE(SUM(array_length(regexp_split_to_array(TRIM("Content"), '\s+'), 1)), 0)::INT AS "TotalWords"
        FROM notes
        GROUP BY TO_CHAR("CreatedAt", 'YYYY-MM')
        ORDER BY "Month" DESC
    """).ToListAsync(cancellationToken);

    var totalNotes = await context.Notes.CountAsync(cancellationToken);

    return new NoteAnalyticsResponse(totalNotes, averageWords, monthlyStats);
}
```

---

## 9. EF Core Migrations: Konsep & Best Practices

### A. Apa itu Migrations & Mengapa Penting?

EF Core Migrations adalah fitur pelacak versi (*version control*) untuk skema database. Setiap kali Anda mengubah atribut entitas di C# (misalnya menambah kolom baru `Category`), Migrations membuatkan script C#/SQL delta untuk memperbarui database tanpa menghapus data yang ada.

> [!WARNING]
> Jangan gunakan `Database.EnsureCreatedAsync()` di *Production environment*! Method `EnsureCreatedAsync()` tidak membuat tabel `__EFMigrationsHistory`, sehingga Anda tidak dapat mengaplikasikan update skema di kemudian hari.

### B. Implementasi `IDesignTimeDbContextFactory`

Agar CLI `dotnet ef` dapat mendeteksi `AppDbContext` dari layer Infrastructure tanpa meng-instantiate seluruh Web API, kita menambahkan [AppDbContextFactory.cs](src/Notes.Infrastructure/Persistence/AppDbContextFactory.cs):

```csharp
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=notesdb;Username=notesuser;Password=notespassword");
        return new AppDbContext(optionsBuilder.Options);
    }
}
```

### C. Best Practices Deployment Migrations

1. **Automated Migration on Startup**: Pada [Program.cs](src/Notes.Api/Program.cs), jalankan migrasi otomatis saat kontainer dinyalakan:
   ```csharp
   using var scope = app.Services.CreateScope();
   var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
   await dbContext.Database.MigrateAsync();
   ```
2. **Kembangkan Skema Secara Non-Breaking**: Jika mengubah nama kolom, buat migrasi dengan alias/backward compatibility terlebih dahulu sebelum menghapus kolom lama.
3. **Pemeriksaan Idempotent**: Gunakan script idempotent SQL (`dotnet ef migrations script --idempotent`) saat melakukan release via CI/CD pipeline Enterprise.

---

## 10. Tips & Langkah Selanjutnya untuk Production

1. **Keamanan Autentikasi**: Ganti Basic Authentication dengan **JWT (JSON Web Token)** atau **OAuth2 / OpenID Connect** dengan Hashed Passwords (seperti BCrypt / Argon2).
2. **Rate Limiting & Caching**: Gunakan `Microsoft.AspNetCore.RateLimiting` dan Caching (Redis / MemoryCache) pada query read-heavy seperti analytics.
3. **Integration Testing**: Buat integration test menggunakan `WebApplicationFactory` dan container PostgreSQL sungguhan (via Testcontainers).

