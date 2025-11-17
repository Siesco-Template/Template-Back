# ImportExportComponent Konfigürasyon Rehberi

## 📋 Genel Bakış

`ImportExportComponent` artık tamamen dinamik çalışıyor ve **`rules.json` dosyası opsiyoneldir**. 

## ✅ Otomatik Rule Generation

Component, Entity Framework Core metadata'sından otomatik olarak validation kuralları oluşturabilir. Bu özellik varsayılan olarak **aktiftir** (`EnableAutoRuleGeneration: true`).

### Nasıl Çalışır?

1. **Öncelik Sırası:**
   - Önce `rules.json` dosyasında tanımlı kurallar kontrol edilir
   - Eğer dosya yoksa veya tablo için kural yoksa, EF Core metadata'sından otomatik kurallar oluşturulur

2. **Otomatik Olarak Tespit Edilenler:**
   - `IsRequired`: Nullable olmayan property'ler için otomatik `true`
   - `MaxLength`: EF Core'da tanımlı `MaxLength` attribute'u veya `StringLength` attribute'u
   - `DataType`: Property tipine göre otomatik (`Int`, `Decimal`, `Date`, `String`)
   - `ForeignTable` ve `ForeignColumn`: Foreign key ilişkileri otomatik tespit edilir

## 📁 rules.json Dosyası (Opsiyonel)

Eğer özel validation kuralları tanımlamak isterseniz, `rules.json` dosyasını oluşturabilirsiniz.

### Dosya Konumu

Varsayılan olarak: `{AppContext.BaseDirectory}/Resources/rules.json`

`appsettings.json`'da `RulesFilePath` ile özelleştirilebilir:
```json
{
  "ImportExport": {
    "RulesFilePath": "Resources/rules.json"  // Relative path
    // veya
    "RulesFilePath": "C:\\CustomPath\\rules.json"  // Absolute path
  }
}
```

### Örnek rules.json

```json
{
  "Users": [
    {
      "Field": "Name",
      "IsRequired": true,
      "MaxLength": 50,
      "DataType": "String"
    },
    {
      "Field": "Email",
      "IsRequired": true,
      "MaxLength": 100,
      "DataType": "String"
    },
    {
      "Field": "Age",
      "DataType": "Int"
    },
    {
      "Field": "DepartmentId",
      "ForeignTable": "Departments",
      "ForeignColumn": "Id"
    }
  ]
}
```

## ⚙️ Konfigürasyon Seçenekleri

`appsettings.json` dosyasına `ImportExport` bölümünü ekleyin:

```json
{
  "ImportExport": {
    "ExportBatchSize": 250000,              // Export için batch boyutu
    "ImportBatchSize": 1000,                // Import için batch boyutu
    "ExportFolderName": "exports",           // Export klasör adı
    "BaseUrl": "https://your-api.com",      // Download link'leri için base URL
    "RulesFilePath": "Resources/rules.json", // rules.json dosya yolu (opsiyonel)
    "EnableAutoRuleGeneration": true,        // Otomatik rule generation (varsayılan: true)
    "MaxRetryAttempts": 3,                  // Hata durumunda retry sayısı
    "RetryDelayMs": 1000,                   // Retry arası bekleme süresi (ms)
    "EnableProgressNotifications": true,     // SignalR progress bildirimleri
    "ProgressNotificationInterval": 1000,    // Progress bildirim aralığı (kayıt sayısı)
    "MaxPreviewRecords": 100,               // Excel preview için max kayıt sayısı
    "EnableTransactionManagement": true,     // Import için transaction yönetimi
    "HeaderDetectionSensitivity": 0.7,      // Excel header tespiti hassasiyeti (0.0-1.0)
    "MaxHeaderRowSearchDepth": 10           // Header satırı için max arama derinliği
  }
}
```

## 🚀 Kullanım Senaryoları

### Senaryo 1: Sadece Otomatik Rule Generation (Önerilen)

`rules.json` dosyasına ihtiyaç yok! Component otomatik olarak EF Core metadata'sından kuralları oluşturur.

```json
{
  "ImportExport": {
    "EnableAutoRuleGeneration": true
  }
}
```

### Senaryo 2: Özel Kurallar ile Birlikte

Bazı tablolar için özel kurallar tanımlamak istiyorsanız:

1. `Resources/rules.json` dosyasını oluşturun
2. Özel kuralları tanımlayın
3. Otomatik generation aktif kalır, sadece tanımlı tablolar için JSON'daki kurallar kullanılır

### Senaryo 3: Sadece JSON Kuralları

Otomatik generation'ı kapatmak istiyorsanız:

```json
{
  "ImportExport": {
    "EnableAutoRuleGeneration": false,
    "RulesFilePath": "Resources/rules.json"
  }
}
```

⚠️ **Not:** Bu durumda `rules.json` dosyası zorunlu hale gelir.

## 📝 Önemli Notlar

1. **rules.json dosyası yoksa hata vermez** - Otomatik generation devreye girer
2. **Otomatik generation aktifse**, JSON'daki kurallar önceliklidir
3. **Foreign key validation** her iki yöntemde de çalışır
4. **Performance:** Otomatik generation ilk çağrıda biraz daha yavaş olabilir, sonrasında cache'lenir

## 🔧 Troubleshooting

### Problem: Validation kuralları çalışmıyor

**Çözüm:**
- `EnableAutoRuleGeneration: true` olduğundan emin olun
- Entity Framework metadata'sının doğru yapılandırıldığından emin olun
- Log'larda hata mesajlarını kontrol edin

### Problem: rules.json dosyası bulunamıyor

**Çözüm:**
- Dosya yolu doğru mu kontrol edin
- `RulesFilePath` konfigürasyonunu kontrol edin
- Dosya yoksa sorun değil - otomatik generation devreye girer


