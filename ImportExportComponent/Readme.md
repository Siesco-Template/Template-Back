# 📦 ImportExportComponent – Məlumatın İdxalı və İxracı Sistemi

## 📘 Layihə Haqqında
**ImportExportComponent** istifadəçilərin məlumatları sistemə yükləməsi (**import**) və sistemdən çıxarması (**export**) üçün hazırlanmış, qayda əsaslı və asinxron işləyən güclü bir moduldur.

Bu modul aşağıdakı əsas məqsədləri yerinə yetirir:
- Excel fayllarını sistemə yükləyərək avtomatik tanımaq və doğrulamaq,
- İstifadəçi tərəfindən təyin edilmiş qaydalara əsasən məlumatları yoxlamaq,
- Arxa planda (background queue) böyük həcmli məlumatları **Excel** formatında ixrac etmək,
- Hər bir istifadəçi üçün fərdi nəticə fayllarını saxlamaq və bildiriş göndərmək.

---

## ⚙️ Əsas Xüsusiyyətlər

- 📥 **Excel Import** – Fayllardan başlıqları avtomatik tanıyır və datanı analiz edir  
- 🧾 **Validation Rules** – `rules.json` faylı vasitəsilə hər cədvəl üçün sahə qaydaları təyin olunur  
- 📤 **Asinxron Export** – Channel və BackgroundService vasitəsilə məlumat ixracını növbəyə alır  
- ⚡ **Mass Data Processing** – 250.000+ sətrlik məlumatlar üçün hissə-hissə işləmə  
- 🔗 **ForeignKey yoxlaması** – Başqa cədvəllərlə əlaqəli dəyərləri dinamik yoxlayır  
- 🧠 **Dynamic Query Builder** – TableQueryRequest əsaslı sorğu yaradılması və ixracı  
- 🔔 **User-based Notification** – Hər istifadəçiyə öz nəticə faylının linki qaytarılır  
- 📊 **OpenXML Export Engine** – RAM sərfiyyatını minimuma endirərək böyük Excel fayllarını yaradır  

---

## 🧩 Layihə Strukturuna Baxış

### 📂 `BackgroundServices`
- **ExportWorkerService.cs**  
  Asinxron növbə mexanizmi (Channel) üzərindən export əməliyyatlarını paralel icra edir.  
  Hər yazı `(columns, tableRequest, userId)` formatında növbəyə əlavə olunur və `ExportQueryHelper` çağırılır.

### ⚙️ `HelperServices`
- **ExportQueryHelper.cs**  
  Export əməliyyatında sorğu (`TableQueryRequest`) və sütun siyahısını (`ExportColumnDto`) alaraq `ExportService`-ə ötürür.  
- **RuleProvider.cs**  
  `rules.json` faylını oxuyur, hər cədvəl üçün sahə qaydalarını (`FieldValidationRule`) qaytarır,  
  həmçinin xarici dəyərlərin (`ForeignTable`, `ForeignColumn`) bazada olub-olmamasını yoxlayır.

### 🧮 `Services`
- **ImportService.cs**  
  - `ExtractExcelPreviewAsync` → Excel faylının başlıqlarını və sətirlərini tanıyır  
  - `ValidateRecords` → Qaydalara əsasən dəyərləri yoxlayır və xəta siyahısı yaradır  
  - `SaveRecords` → Validated məlumatları `EntitySetProvider` vasitəsilə DB-yə yazır  
- **ExportService.cs**  
  - `ExportAndNotifyAsync` → Verilən sorğu və sütunlara əsasən Excel faylı yaradır  
  - `ExportDataAsync` → 250.000-lik partiyalarda məlumatları yazır  
  - `WriteHeader`, `WriteData` → OpenXML əsasında sətir-sətir yazma prosesi  

---

## 🧱 DTO Layihə Qatları

| DTO | İzah |
|------|------|
| **ExportRequestDto** | İxrac üçün sütun və TableQueryRequest məlumatlarını saxlayır |
| **ExportColumnDto** | Hər sütun üçün PropertyName və Header dəyərləri |
| **ExcelPreviewDto** | Import öncəsi Excel başlıqları və dataların nümunəsi |
| **ValidateRequestDto** | Validasiya üçün Records və Mappings dəyərlərini saxlayır |
| **ValidatedRecordDto** | Hər sətirin doğruluğu və səhv siyahısı |
| **ImportConfirmRequestDto** | Yükləmə təsdiqi üçün məlumatlar (Records, TableName) |
| **ImportResult** | Uğurlu import nəticələrinin sayı |
| **FieldValidationRule** | Qaydaların tərifi (IsRequired, MaxLength, DataType, ForeignKey və s.) |

---

## 📘 Qaydalar Sistemi (rules.json nümunəsi)

```json
{
  "Users": [
    { "Field": "Name", "IsRequired": true, "MaxLength": 50, "DataType": "String" },
    { "Field": "Email", "IsRequired": true, "MaxLength": 100, "DataType": "String" },
    { "Field": "Age", "DataType": "Int" },
    { "Field": "DepartmentId", "ForeignTable": "Departments", "ForeignColumn": "Id" }
  ]
}
```

Bu struktur sistemə **dinamik validasiya** dəstəyi verir:  
- `IsRequired` → boşluq yoxlaması  
- `MaxLength` → maksimum uzunluq  
- `DataType` → tip yoxlaması (`Int`, `Decimal`, `Date` və s.)  
- `ForeignTable` + `ForeignColumn` → başqa cədvəldə dəyərin olub-olmamasını yoxlayır  

---

## 🚀 Əməliyyat Axını

### 🔹 **Import Prosesi**
```
Excel yüklənir → Başlıqlar avtomatik tanınır → Validasiya edilir → 
İstifadəçi təsdiq edir → Məlumat DB-yə yazılır
```

### 🔹 **Export Prosesi**
```
Frontend API → ExportRequestDto → Queue.WriteAsync → 
ExportWorkerService → ExportService → Excel faylı yaradılır → 
DownloadList-ə link əlavə olunur → İstifadəçiyə bildiriş
```

---

## ⚙️ Texnologiyalar

| Texnologiya | İstifadə məqsədi |
|--------------|------------------|
| **ASP.NET Core 8.0** | API əsaslı servis arxitekturası |
| **ClosedXML** | Excel fayllarının oxunması |
| **OpenXML SDK** | Böyük Excel fayllarının yaddaş dostu yazılması |
| **Entity Framework Core** | DB əməliyyatları və LINQ sorğuları |
| **BackgroundService + Channel** | Asinxron export növbəsi |
| **JsonSerializer / Newtonsoft.Json** | Qaydaların və dataların işlənməsi |
| **LINQ Expressions** | Dinamik validasiya və sorğu icrası |

---

## 🧩 İstifadə Nümunələri

### 🔹 **Excel Yükləmə və Önizləmə**
```csharp
var preview = await _importService.ExtractExcelPreviewAsync(file);
```

### 🔹 **Validasiya**
```csharp
var result = await _importService.ValidateRecords(new ValidateRequestDto
{
    TableName = "Users",
    Records = preview.Records,
    Mappings = new() { { "Ad", "Name" }, { "E-poçt", "Email" } }
});
```

### 🔹 **İdxalı Təsdiqlə**
```csharp
await _importService.SaveRecords(new ImportConfirmRequestDto
{
    TableName = "Users",
    Records = validRecords.Select(v => v.Records).ToList()
});
```

### 🔹 **İxrac Əməliyyatı**
```csharp
await exportQueue.Writer.WriteAsync((columns, tableRequest, userId));
```

---

## 🧑‍💻 Müəllif
Hazırlayan: **Siesco Studio**  
📧 Əlaqə: [info@siescostudio.az](mailto:info@siescostudio.az)  
🏗 Layihə: Import/Export modulunun .NET əsaslı tətbiqi  

---
