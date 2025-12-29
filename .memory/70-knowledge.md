# Domain Bilgisi

## Anahtar Kavramlar

### Kullanıcı Rolleri
| Rol | Türkçe | Yetki |
|-----|--------|-------|
| Citizen | Vatandaş | Şikayet oluştur, takip et |
| InstitutionRepresentative | Kurum Temsilcisi | Şikayet yönet |
| NGOCoordinator | STÖ Koordinatörü | Vatandaşları destekle |
| Moderator | Moderatör | İçerik incele |
| Admin | Yönetici | Tam kontrol |

### Şikayet Durumları
| Durum | Açıklama |
|-------|----------|
| PendingModeration | Moderatör incelemesinde |
| Approved | Onaylandı, kuruma gönderildi |
| Rejected | Reddedildi |
| InProgress | Kurum işlemde |
| Resolved | Çözüldü |
| Closed | Kapatıldı |

### Şikayet Türleri
- Suçlama
- İmam
- Sağlık
- Eğitim
- Ulaştırma
- Harita
- Diğer

### Rozet Sistemi
| Rozet | Koşul |
|-------|-------|
| Söz Sahibi | 1. başvuru |
| Fark Yaratıcı | 3 başvuru |
| Sürekli Çabalar | 10 başvuru |
| Değişim Habercisi | 25 başvuru |
| Toplum Savunucusu | 50 başvuru |
| İçerik Ustası | 5 medya dosyalı başvuru |
| Hızlı Çözüm | 3 gün içinde çözülen |
| Yaklaşıklık | Aynı kuruma 5+ başvuru |
| Hataboyu | Yanlış ihbar uyarıları |
| Karşılaştırılmaması | Uygunsuz içerik uyarıları |

## Dosya Limitleri
| Tür | Max Boyut | Max Adet | Formatlar |
|-----|-----------|----------|-----------|
| Fotoğraf | 10MB | 5 | JPG, PNG, GIF |
| Video | 100MB | 2 | MP4, MOV, AVI |
| Ses | 20MB | 2 | MP3, WAV, OGG |

## Uyarı Sistemi
1. **1. Uyarı**: İçerik kaldırılır, bildirim
2. **2. Uyarı**: 7 gün hesap askıya
3. **3. Uyarı**: 30 gün hesap askıya
4. **4. Uyarı**: Kalıcı yasaklama

## Kurum Kodları
PDF'e göre kurumlar sistem tarafından tanımlanır ve Admin onayı ile temsilciler atanır.

## En İyi Pratikler
1. Her şikayet en az başlık ve açıklama içermeli
2. Medya dosyaları virüs taramasından geçmeli
3. Anonim gönderimler desteklenmeli
4. Durum değişikliklerinde bildirim gönderilmeli

## SSS
**S**: Anonim şikayet gönderilebilir mi?  
**C**: Evet, "Anonim Olarak Gönder" checkbox'ı mevcuttur.

**S**: Kurum temsilcisi nasıl olunur?  
**C**: Kayıt sırasında kurum seçilir, Admin onayı beklenir.

**S**: Şikayet düzenlenebilir mi?  
**C**: Sadece "Moderatör İncelemesinde" durumundayken düzenlenebilir.
