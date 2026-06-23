# MyApi - ASP.NET Core API Template

Template ini adalah starter project untuk membuat REST API dengan ASP.NET Core 8, Entity Framework Core, PostgreSQL, Serilog, Swagger, dan struktur layer sederhana:

- `Controllers` untuk menerima HTTP request.
- `Services` untuk business logic.
- `Repositories` untuk akses database.
- `Models` untuk entity database.
- `DTOs` untuk request/response object.
- `Data` untuk `DbContext`.
- `Middleware` untuk custom middleware global.
- `Migrations` untuk riwayat perubahan schema database.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Serilog
- Swagger / OpenAPI

## Struktur Folder

```text
MyApi/
├── Controllers/
│   ├── BaseController.cs
│   └── UserController.cs
├── Data/
│   └── AppDbContext.cs
├── DTOs/
│   ├── ApiRespose.cs
│   └── UserDto.cs
├── Middleware/
│   └── ExpectionMiddleware.cs
├── Migrations/
├── Models/
│   └── User.cs
├── Repositories/
│   ├── Interface/
│   └── UserRepository.cs
├── Services/
│   ├── Interface/
│   └── UserService.cs
├── Program.cs
├── appsettings.json
└── MyApi.csproj
```

> Catatan: file `ApiRespose.cs` dan `ExpectionMiddleware.cs` kemungkinan typo dari `ApiResponse.cs` dan `ExceptionMiddleware.cs`. Project tetap bisa berjalan selama nama class dan namespace benar, tetapi sebaiknya dirapikan ketika project sudah stabil.

## Cara Menjalankan Project

Pastikan sudah terinstall:

- .NET SDK 8
- PostgreSQL
- EF Core CLI

Install EF Core CLI jika belum ada:

```bash
dotnet tool install --global dotnet-ef
```

Restore dependency:

```bash
dotnet restore
```

Atur koneksi database di `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=dbdotnet;Username=ndikdev;Password=password123"
}
```

Jalankan migration ke database:

```bash
dotnet ef database update
```

Jalankan aplikasi:

```bash
dotnet run
```

Saat environment `Development`, Swagger dapat dibuka di:

```text
https://localhost:<port>/swagger
```

Port dapat dilihat di `Properties/launchSettings.json` atau output terminal saat `dotnet run`.

## Konsep Alur Request

Alur utama template ini:

```text
Client
  -> Controller
  -> Service
  -> Repository
  -> AppDbContext
  -> Database
```

Contoh pada fitur `User`:

- `UserController` menerima request `/api/user`.
- `UserService` memproses business logic.
- `UserRepository` membaca/menulis data memakai EF Core.
- `AppDbContext` menghubungkan entity `User` dengan tabel database.

Response dibuat konsisten memakai `ApiResponse<T>` melalui helper di `BaseController`.

## Migration Database

Migration digunakan untuk mencatat perubahan struktur database dari entity di folder `Models`.

### Membuat Migration Baru

Contoh ketika menambah field baru pada model `User`:

```csharp
public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
```

Buat migration:

```bash
dotnet ef migrations add AddPhoneNumberToUser
```

Terapkan ke database:

```bash
dotnet ef database update
```

### Melihat Daftar Migration

```bash
dotnet ef migrations list
```

### Rollback Migration

Rollback ke migration tertentu:

```bash
dotnet ef database update NamaMigrationSebelumnya
```

Jika migration terakhir belum pernah diterapkan atau ingin dihapus dari kode:

```bash
dotnet ef migrations remove
```

## Menambah API Baru

Contoh menambah fitur `Product`.

### 1. Buat Model

Buat file `Models/Product.cs`:

```csharp
namespace MyApi.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

### 2. Tambahkan DbSet

Edit `Data/AppDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }
```

### 3. Buat DTO

Buat file `DTOs/ProductDto.cs`:

```csharp
namespace MyApi.DTOs
{
    public class CreateProductDto
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

### 4. Buat Repository Interface

Buat file `Repositories/Interface/IProductRepository.cs`:

```csharp
using MyApi.Models;

namespace MyApi.Repositories.Interface
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAll();
        Task<Product?> GetById(Guid id);
        Task<Product> Create(Product product);
        Task Update(Product product);
        Task Delete(Product product);
    }
}
```

### 5. Buat Repository

Buat file `Repositories/ProductRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using MyApi.Repositories.Interface;

namespace MyApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAll()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetById(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> Create(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task Update(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
```

### 6. Buat Service Interface

Buat file `Services/Interface/IProductService.cs`:

```csharp
using MyApi.DTOs;
using MyApi.Models;

namespace MyApi.Services.Interface
{
    public interface IProductService
    {
        Task<List<Product>> GetAll();
        Task<Product?> GetById(Guid id);
        Task<Product> Create(CreateProductDto dto);
        Task<bool> Update(Guid id, UpdateProductDto dto);
        Task<bool> Delete(Guid id);
    }
}
```

### 7. Buat Service

Buat file `Services/ProductService.cs`:

```csharp
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Repositories.Interface;
using MyApi.Services.Interface;

namespace MyApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, ILogger<ProductService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<Product>> GetAll()
        {
            _logger.LogInformation("Fetching all products");
            return await _repo.GetAll();
        }

        public async Task<Product?> GetById(Guid id)
        {
            return await _repo.GetById(id);
        }

        public async Task<Product> Create(CreateProductDto dto)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Price = dto.Price
            };

            return await _repo.Create(product);
        }

        public async Task<bool> Update(Guid id, UpdateProductDto dto)
        {
            var product = await _repo.GetById(id);
            if (product == null) return false;

            product.Name = dto.Name;
            product.Price = dto.Price;

            await _repo.Update(product);
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var product = await _repo.GetById(id);
            if (product == null) return false;

            await _repo.Delete(product);
            return true;
        }
    }
}
```

### 8. Daftarkan Dependency Injection

Edit `Program.cs`:

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

Pastikan namespace repository dan service sudah di-import jika diperlukan:

```csharp
using MyApi.Repositories;
using MyApi.Repositories.Interface;
using MyApi.Services;
using MyApi.Services.Interface;
```

### 9. Buat Controller

Buat file `Controllers/ProductController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using MyApi.DTOs;
using MyApi.Services.Interface;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : BaseController
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            return Success(data, "Success get data products");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _service.GetById(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            return Success(product, "Success get data product");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var product = await _service.Create(dto);
            return Created(product, "Success created");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateProductDto dto)
        {
            var result = await _service.Update(id, dto);
            if (!result) return NotFoundResponse("Product not found");

            return Success("OK", "Product updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.Delete(id);
            if (!result) return NotFoundResponse("Product not found");

            return Success("OK", "Product deleted");
        }
    }
}
```

### 10. Buat Migration

```bash
dotnet ef migrations add AddProduct
dotnet ef database update
```

Endpoint baru akan muncul otomatis di Swagger.

## Pagination, Filtering, dan Sorting

Template ini menyediakan pola query reusable untuk berbagai tabel melalui:

- `DTOs/PaginationDto.cs`
- `Extensions/QueryableExtensions.cs`
- Method repository seperti `GetPaged(QueryParameters parameters)`

Endpoint contoh sudah tersedia di:

```text
GET /api/user/paged
```

### Parameter Query

Parameter yang tersedia:

- `pageNumber` untuk nomor halaman.
- `pageSize` untuk jumlah data per halaman. Maksimal default `100`.
- `search` untuk pencarian umum pada semua property bertipe `string`.
- `sortBy` untuk nama kolom yang ingin diurutkan.
- `sortDirection` untuk arah sorting, isi `asc` atau `desc`.
- `filters[NamaKolom]` untuk filter spesifik per kolom.

Contoh:

```text
GET /api/user/paged?pageNumber=1&pageSize=10
```

Search semua kolom string:

```text
GET /api/user/paged?search=budi
```

Sorting:

```text
GET /api/user/paged?sortBy=name&sortDirection=asc
```

Filter spesifik:

```text
GET /api/user/paged?filters[Name]=budi&filters[Email]=gmail
```

Gabungan pagination, search, filter, dan sort:

```text
GET /api/user/paged?pageNumber=1&pageSize=10&search=budi&sortBy=email&sortDirection=desc
```

Response paging:

```json
{
  "success": true,
  "message": "Success get paged users",
  "data": {
    "items": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 0,
    "totalPages": 0,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "errors": null
}
```

### Integrasi Paging ke Tabel Lain

Untuk tabel baru seperti `Product`, tambahkan method di repository interface:

```csharp
Task<PagedResult<Product>> GetPaged(QueryParameters parameters);
```

Implementasi di repository:

```csharp
public async Task<PagedResult<Product>> GetPaged(QueryParameters parameters)
{
    return await _context.Products
        .AsNoTracking()
        .ApplySearch(parameters.Search)
        .ApplyFilters(parameters.Filters)
        .ApplySorting(parameters.SortBy, parameters.SortDirection)
        .ToPagedResultAsync(parameters);
}
```

Tambahkan method di service interface:

```csharp
Task<PagedResult<Product>> GetPaged(QueryParameters parameters);
```

Implementasi di service:

```csharp
public async Task<PagedResult<Product>> GetPaged(QueryParameters parameters)
{
    return await _repo.GetPaged(parameters);
}
```

Tambahkan endpoint di controller:

```csharp
[HttpGet("paged")]
public async Task<IActionResult> GetPaged([FromQuery] QueryParameters parameters)
{
    var data = await _service.GetPaged(parameters);
    return Success(data, "Success get paged products");
}
```

Dengan pola ini, fitur paging bisa dipakai untuk banyak tabel tanpa menulis ulang logic search, filter, sort, skip, dan take.

## Custom Middleware

Template ini sudah memiliki global exception middleware di `Middleware/ExpectionMiddleware.cs`.

Middleware tersebut menangkap error dari controller/service/repository lalu mengubahnya menjadi response JSON yang konsisten.

Contoh mapping error:

- `KeyNotFoundException` menjadi HTTP `404`.
- `ArgumentException` menjadi HTTP `400`.
- `UnauthorizedAccessException` menjadi HTTP `401`.
- Error lain menjadi HTTP `500`.

Registrasi middleware ada di `Program.cs`:

```csharp
app.UseMiddleware<ExceptionMiddleware>();
```

Urutan middleware penting. Untuk global exception handler, letakkan sebelum middleware lain yang berpotensi menghasilkan error.

### Menambah Middleware Baru

Contoh membuat middleware untuk menambahkan header response.

Buat file `Middleware/RequestHeaderMiddleware.cs`:

```csharp
namespace MyApi.Middleware
{
    public class RequestHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers["X-App-Name"] = "MyApi";
            await _next(context);
        }
    }
}
```

Daftarkan di `Program.cs`:

```csharp
app.UseMiddleware<RequestHeaderMiddleware>();
```

## Logging

Project ini memakai Serilog. Konfigurasi ada di `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

Log akan ditulis ke:

- Console.
- File harian di folder `logs/`.

Contoh penggunaan logger di service:

```csharp
private readonly ILogger<ProductService> _logger;

public ProductService(ILogger<ProductService> logger)
{
    _logger = logger;
}

public void Example()
{
    _logger.LogInformation("Product service is running");
}
```

Level log yang umum digunakan:

- `LogTrace` untuk detail sangat rendah.
- `LogDebug` untuk debugging development.
- `LogInformation` untuk aktivitas normal.
- `LogWarning` untuk kondisi tidak normal tetapi aplikasi masih berjalan.
- `LogError` untuk error yang perlu diperiksa.
- `LogCritical` untuk error fatal.

Jika ingin log semua HTTP request otomatis, aktifkan baris ini di `Program.cs`:

```csharp
app.UseSerilogRequestLogging();
```

Letakkan setelah exception middleware atau sebelum routing sesuai kebutuhan observability aplikasi.

## Monitoring

Monitoring minimal yang sudah tersedia:

- Log request/error melalui Serilog.
- File log harian di folder `logs`.
- Swagger untuk inspeksi endpoint saat development.

Rekomendasi untuk production:

- Kirim log ke centralized logging seperti Seq, ELK, Grafana Loki, Datadog, atau Application Insights.
- Tambahkan health check endpoint.
- Tambahkan metrics dengan OpenTelemetry jika diperlukan.

Contoh health check sederhana:

```csharp
builder.Services.AddHealthChecks();
```

Tambahkan endpoint:

```csharp
app.MapHealthChecks("/health");
```

Setelah itu cek:

```text
GET /health
```

## Swagger

Swagger dikonfigurasi di `Program.cs`:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

Swagger UI hanya aktif saat `Development`:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Root `/` juga diarahkan ke `/swagger` saat development:

```csharp
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});
```

Untuk memastikan Swagger muncul:

1. Jalankan aplikasi dengan environment `Development`.
2. Buka `/swagger`.
3. Pastikan controller diberi `[ApiController]` dan route seperti `[Route("api/[controller]")]`.

Swagger juga sudah dikonfigurasi untuk JWT Bearer. Klik tombol `Authorize`, lalu masukkan token dengan format:

```text
Bearer <token>
```

## Authentication dan Authorization JWT

Template ini sudah memakai JWT Bearer Authentication.

Package yang dipakai:

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`

Konfigurasi ada di `appsettings.json`:

```json
"Jwt": {
  "Key": "CHANGE_THIS_SECRET_KEY_MINIMUM_32_CHARACTERS",
  "Issuer": "MyApi",
  "Audience": "MyApiClient",
  "ExpiresInMinutes": 60
},
"DemoAuth": {
  "Username": "admin",
  "Password": "admin123"
}
```

> Untuk production, ganti `Jwt:Key` dengan secret yang kuat dan simpan melalui environment variable atau secret manager.

### Login

Endpoint login demo:

```text
POST /api/auth/login
```

Body:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

Response:

```json
{
  "success": true,
  "message": "Login success",
  "data": {
    "token": "<jwt-token>",
    "expiresAt": "2026-06-23T10:00:00Z"
  },
  "errors": null
}
```

Gunakan token tersebut untuk mengakses endpoint yang diberi `[Authorize]`.

Header:

```text
Authorization: Bearer <jwt-token>
```

Contoh curl:

```bash
curl -X GET https://localhost:<port>/api/user \
  -H "Authorization: Bearer <jwt-token>"
```

### Melindungi Controller atau Endpoint

Melindungi satu controller:

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : BaseController
{
}
```

Melindungi satu endpoint:

```csharp
[Authorize]
[HttpPost]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    var product = await _service.Create(dto);
    return Created(product, "Success created");
}
```

Membuka endpoint tertentu tanpa login:

```csharp
[AllowAnonymous]
[HttpGet("public")]
public IActionResult PublicEndpoint()
{
    return Success("OK", "Public endpoint");
}
```

### Role-Based Authorization

Token demo dibuat dengan role `Admin`. Untuk membatasi endpoint berdasarkan role:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    var result = await _service.Delete(id);
    if (!result) return NotFoundResponse("Data not found");

    return Success("OK", "Data deleted");
}
```

### Menghubungkan Login ke Tabel User

Login demo saat ini membaca username/password dari konfigurasi `DemoAuth`. Untuk aplikasi nyata, ganti logic di `AuthController` menjadi:

- Cari user berdasarkan email/username dari database.
- Verifikasi password hash, jangan simpan password plain text.
- Ambil role/permission user.
- Generate JWT melalui `IJwtTokenService`.

Contoh alur production:

```text
POST /api/auth/login
  -> AuthController
  -> AuthService
  -> UserRepository
  -> Verify password hash
  -> JwtTokenService.GenerateToken(username, roles)
```

## Response Standard

Template ini memakai format response:

```json
{
  "success": true,
  "message": "Success",
  "data": {},
  "errors": null
}
```

Untuk error:

```json
{
  "success": false,
  "message": "Data not found",
  "data": null,
  "errors": null
}
```

Gunakan helper dari `BaseController`:

```csharp
return Success(data, "Success get data");
return Created(data, "Success created");
return BadRequestResponse("Invalid request");
return NotFoundResponse("Data not found");
return UnauthorizedResponse("Unauthorized");
return ForbiddenResponse("Forbidden");
return ServerErrorResponse("Internal Server Error");
```

## Konfigurasi Environment

Gunakan:

- `appsettings.json` untuk konfigurasi umum.
- `appsettings.Development.json` untuk konfigurasi development.
- Environment variable untuk secret di production.

Jangan commit password production, token, private key, atau credential asli ke repository.

Contoh menjalankan dengan environment Development:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

## Testing API

Melalui Swagger:

```text
GET    /api/user
GET    /api/user/paged
GET    /api/user/{id}
POST   /api/user
PUT    /api/user/{id}
DELETE /api/user/{id}
```

Contoh body untuk membuat user:

```json
{
  "name": "Budi",
  "email": "budi@example.com"
}
```

Melalui curl:

```bash
curl -X GET https://localhost:<port>/api/user
```

```bash
curl -X POST https://localhost:<port>/api/user \
  -H "Content-Type: application/json" \
  -d '{"name":"Budi","email":"budi@example.com"}'
```

## Build dan Publish

Build project:

```bash
dotnet build
```

Publish untuk production:

```bash
dotnet publish -c Release -o ./publish
```

Jalankan hasil publish:

```bash
dotnet ./publish/MyApi.dll
```

## Unit Test

Template ini memiliki test project:

```text
MyApi.Tests/
```

Test yang sudah tersedia:

- `QueryableExtensionsTests` untuk memastikan pagination, search, filter, dan sorting berjalan benar.
- `UserRepositoryTests` untuk memastikan repository bisa memakai query paging reusable.
- `UserServiceTests` untuk memastikan service memanggil repository dan mengembalikan hasil sesuai kontrak.

Jalankan semua test:

```bash
dotnet test
```

Menjalankan test project saja:

```bash
dotnet test MyApi.Tests/MyApi.Tests.csproj
```

Saat menambah fitur baru, pola test yang disarankan:

- Test service memakai fake repository agar business logic mudah diuji.
- Test repository memakai EF Core InMemory untuk flow query sederhana.
- Test helper reusable seperti pagination/filter/sort secara terpisah.
- Tambahkan minimal test untuk create, update, delete, not found, dan query paging.

## Checklist Saat Menambah Fitur Baru

- Buat model di `Models`.
- Tambahkan `DbSet` di `AppDbContext`.
- Buat DTO di `DTOs`.
- Buat repository interface.
- Buat repository implementation.
- Buat service interface.
- Buat service implementation.
- Daftarkan dependency injection di `Program.cs`.
- Buat controller.
- Buat migration.
- Jalankan `dotnet ef database update`.
- Test endpoint di Swagger.
- Tambahkan unit test untuk service, repository, dan query penting.
- Tambahkan logging pada proses penting.
- Pastikan error dikembalikan lewat response standar.

## Troubleshooting

### Database tidak connect

Periksa:

- PostgreSQL sudah running.
- Database sudah dibuat.
- Host, port, username, password, dan database di connection string benar.
- User database punya akses ke database.

### Command `dotnet ef` tidak ditemukan

Install EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

Jika sudah pernah install:

```bash
dotnet tool update --global dotnet-ef
```

### Swagger tidak muncul

Periksa:

- Environment adalah `Development`.
- `app.UseSwagger()` dan `app.UseSwaggerUI()` aktif.
- Controller menggunakan `[ApiController]`.
- Route controller benar.

### Migration gagal

Periksa:

- Project berhasil build dengan `dotnet build`.
- Connection string benar.
- Entity dan `DbSet` sudah sesuai.
- Nama migration belum pernah dipakai.

## Catatan Pengembangan

Beberapa improvement yang bisa ditambahkan berikutnya:

- Validasi DTO dengan Data Annotations atau FluentValidation.
- Integration test untuk endpoint HTTP lengkap.
- Dockerfile dan docker-compose untuk API + PostgreSQL.
- Health check dan OpenTelemetry untuk monitoring production.
