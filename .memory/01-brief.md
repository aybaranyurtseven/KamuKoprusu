# Kamu Köprüsü - Proje Özeti

## Proje Vizyonu
Vatandaşların kamu kurumlarına şikayet ve önerilerini iletebildiği, medya dosyaları ekleyebildiği ve kurumların çözüm süreçlerini takip edebildiği dijital platform.

## Temel Gereksinimler

### Zorunlu Özellikler
1. **Kullanıcı Yönetimi**: 5 rol (Vatandaş, Kurum Temsilcisi, STÖ Koordinatörü, Moderatör, Admin)
2. **Şikayet/Öneri Sistemi**: Oluşturma, takip, güncelleme
3. **Medya Desteği**: Fotoğraf (max 5x10MB), Video (max 2x100MB), Ses (max 2x20MB)
4. **Badge/Rozet Sistemi**: 10 farklı rozet, 5 seviye (Bronze → Diamond)
5. **Moderasyon**: İçerik inceleme, uyarı sistemi (4 aşamalı)

### Başarı Kriterleri
- Vatandaşlar şikayet/öneri oluşturabilmeli
- Kurumlar başvuruları yönetebilmeli
- Admin sistem genelini kontrol edebilmeli
- Rozet ve seviye sistemi çalışmalı

## Paydaşlar
- **Vatandaşlar**: Son kullanıcılar
- **Kamu Kurumları**: Şikayetleri yanıtlayan kurumlar
- **STÖ'ler**: Sivil toplum kuruluşları
- **Sistem Yöneticileri**: Admin ve Moderatörler

## Kısıtlamalar
- Yerel dosya depolama (Azure/AWS entegrasyonu opsiyonel)
- E-posta/SMS servisleri placeholder
- SignalR ilk sürümde yok

## Zaman Çizelgesi
- **Mevcut**: Temel altyapı tamamlandı (Identity, DbContext, Models)
- **Hedef**: Tam fonksiyonel MVP
