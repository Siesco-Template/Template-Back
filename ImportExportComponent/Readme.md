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

### 📡 `Hubs`
- **ExportHub.cs**  
  SignalR vasitəsilə real vaxtda istifadəçiyə məlumat göndərmək üçün istifadə olunur.

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
