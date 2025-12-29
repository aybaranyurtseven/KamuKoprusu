# Sistem Mimarisi

## Genel Bakış

```mermaid
graph TD
    subgraph "Presentation Layer"
        VIEWS[Razor Views]
        CONTROLLERS[MVC Controllers]
        API[API Controllers]
    end
    
    subgraph "Business Layer"
        SERVICES[Services]
        VALIDATORS[Validators]
    end
    
    subgraph "Data Layer"
        DBCONTEXT[ApplicationDbContext]
        IDENTITY[ASP.NET Identity]
    end
    
    subgraph "Database"
        SQLSERVER[(SQL Server)]
    end
    
    VIEWS --> CONTROLLERS
    CONTROLLERS --> SERVICES
    CONTROLLERS --> DBCONTEXT
    API --> DBCONTEXT
    SERVICES --> DBCONTEXT
    DBCONTEXT --> SQLSERVER
    IDENTITY --> SQLSERVER
```

## Bileşen Dökümü

### Controllers (7+)
| Controller | Rol | Sayfalar |
|------------|-----|----------|
| HomeController | Genel | Index |
| AccountController | Auth | Login, Register, Logout |
| CitizenController | Vatandaş | Dashboard, CreateComplaint, MyComplaints |
| InstitutionController | Kurum | Dashboard, IncomingComplaints, ManageComplaint |
| AdminController | Admin | Dashboard, Users, Complaints |
| ModeratorController | Moderatör | Dashboard, ReviewContent |
| NGOController | STÖ | Dashboard, Profile |

### Models (12 Entity)
| Model | İlişkiler |
|-------|-----------|
| ApplicationUser | → Profile, Complaints, Badges, Warnings |
| Profile | → User |
| Institution | → Complaints |
| NGO | → Users |
| Complaint | → User, Institution, Media, Updates |
| ComplaintUpdate | → Complaint |
| Media | → Complaint |
| Badge | → UserBadges |
| UserBadge | → User, Badge |
| Warning | → User |
| BannedUser | → User |
| AuditLog | - |

### Enums (5)
- ComplaintStatus
- ComplaintType
- MediaType
- UserLevel
- UserRole

## Veritabanı İlişkileri

```mermaid
erDiagram
    ApplicationUser ||--o{ Complaint : creates
    ApplicationUser ||--o| Profile : has
    ApplicationUser ||--o{ UserBadge : earns
    ApplicationUser ||--o{ Warning : receives
    
    Complaint ||--o{ Media : contains
    Complaint ||--o{ ComplaintUpdate : has
    Complaint }o--|| Institution : targets
    
    Badge ||--o{ UserBadge : awarded_as
    
    Institution ||--o{ ApplicationUser : employs
    NGO ||--o{ ApplicationUser : coordinates
```

## Güvenlik Mimarisi
- CSRF koruması (ValidateAntiForgeryToken)
- Şifre gereksinimleri (8+ karakter, mixed case, digit, special)
- Hesap kilitleme (5 deneme → 10 dakika)
- Role-based access control
- Policy-based authorization
