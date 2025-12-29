# Teknoloji Yığını

## Framework ve Araçlar

| Kategori | Teknoloji | Versiyon |
|----------|-----------|----------|
| Framework | ASP.NET Core MVC | .NET 10.0 |
| ORM | Entity Framework Core | 10.0.0 |
| Veritabanı | SQL Server (LocalDB) | - |
| Auth | ASP.NET Core Identity | 10.0.0 |
| API Docs | Swagger/OpenAPI | 7.2.0 |

## NuGet Paketleri

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="7.2.0" />
```

## Geliştirme Ortamı

### Gereksinimler
- .NET 10.0 SDK
- Visual Studio 2022+ veya VS Code
- SQL Server LocalDB

### Çalıştırma
```powershell
cd "c:\Users\PC1\Desktop\Project\KamuKoprusu2"
dotnet run
```

### URL'ler
- HTTP: https://localhost:5001
- Swagger: https://localhost:5001/swagger

## Dosya Yapısı (Hedef)

```
KamuKoprusu2/
├── Controllers/         # MVC + API Controllers
│   ├── Api/
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── CitizenController.cs
│   ├── HomeController.cs
│   ├── InstitutionController.cs
│   ├── ModeratorController.cs
│   └── NGOController.cs
├── Data/                # DbContext, Initializer
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Enums/               # Enum definitions
├── Migrations/          # EF Core migrations
├── Models/              # Entity + View models
├── Services/            # Helper services
├── Validators/          # Validation logic
├── Views/               # Razor views
├── wwwroot/             # Static files
├── Program.cs           # App entry point
├── appsettings.json     # Configuration
└── KamuKoprusu2.csproj  # Project file
```

## Konfigürasyon

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KamuKoprusu;Trusted_Connection=True;"
  },
  "FileUpload": {
    "MaxPhotoSizeMB": 10,
    "MaxVideoSizeMB": 100,
    "MaxAudioSizeMB": 20,
    "MaxPhotos": 5,
    "MaxVideos": 2,
    "MaxAudios": 2
  }
}
```

## Build & Deploy
```powershell
# Development
dotnet build
dotnet run

# Production
dotnet publish -c Release
```
