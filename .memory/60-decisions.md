# Karar Logu

## Karar Kayıtları

### KRR-001: Teknoloji Seçimi
**Tarih**: 2024-12  
**Bağlam**: Proje teknoloji yığını belirlenmesi  
**Seçenekler**:
1. ASP.NET Core MVC + EF Core
2. Node.js + Express
3. Django

**Karar**: ASP.NET Core MVC + Entity Framework Core  
**Gerekçe**: 
- PDF dokümantasyonunda belirtilen teknoloji
- Güçlü Identity sistemi
- SQL Server entegrasyonu kolay

---

### KRR-002: Proje Mimarisi (Orijinal)
**Tarih**: 2024-12  
**Bağlam**: Kod organizasyonu  
**Seçenekler**:
1. 3-katmanlı (Core, Infrastructure, Web)
2. Tek proje MVC
3. Clean Architecture

**Karar**: 3-katmanlı mimari  
**Gerekçe**: Sorumlulukların net ayrımı

---

### KRR-003: Proje Mimarisi Değişikliği
**Tarih**: 2024-12-28  
**Bağlam**: Visual Studio standart yapısına geçiş talebi  
**Seçenekler**:
1. 3-katmanlı yapıyı koru
2. Tek proje MVC'ye dönüştür

**Karar**: Tek proje MVC yapısına dönüştür  
**Gerekçe**: 
- Kullanıcı talebi
- Daha basit yapı
- Kolay bakım

---

### KRR-004: Veritabanı Seçimi
**Tarih**: 2024-12  
**Bağlam**: Veritabanı teknolojisi  
**Seçenekler**:
1. SQL Server LocalDB
2. PostgreSQL
3. SQLite

**Karar**: SQL Server LocalDB  
**Gerekçe**: 
- Visual Studio entegrasyonu
- Production'a kolay geçiş
- EF Core desteği mükemmel

---

### KRR-005: Kimlik Doğrulama
**Tarih**: 2024-12  
**Bağlam**: Auth sistemi  
**Seçenekler**:
1. ASP.NET Core Identity
2. Custom auth
3. OAuth/OIDC

**Karar**: ASP.NET Core Identity  
**Gerekçe**: 
- Rollerle entegrasyon
- Güvenlik özellikleri hazır
- Hesap kilitleme desteği
