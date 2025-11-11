# 📂 Folder Service – Qovluq və Fayl İdarəetmə Sistemi

## 📘 Layihə Haqqında
**Folder Service** layihəsi MongoDB əsaslı qovluq və fayl idarəetmə sistemidir.  
Bu xidmət, sistemdəki sənəd strukturlarını ağac (tree) formatında saxlamağa, idarə etməyə və sinxron şəkildə yeniləməyə imkan yaradır.

Layihə mikrosxidmət memarlığına uyğun hazırlanmışdır və digər servislər tərəfindən fayl və qovluq əməliyyatlarının (yaratma, silmə, köçürmə, kopyalama və s.) mərkəzi idarəetməsi üçün istifadə olunur.

---

## ⚙️ Əsas Xüsusiyyətlər

- 🗂 **Qovluq idarəetməsi**  
  Qovluq yaratmaq, silmək, ad dəyişmək, köçürmək və kopyalama funksiyaları.

- 📁 **Fayl idarəetməsi**  
  Fayl əlavə etmək, ad dəyişmək, silmək, köçürmək və kopyalama funksiyaları.

- 🪶 **Dərin struktur dəstəyi (recursive)**  
  Qovluqlar və alt qovluqlar bir ağac strukturu kimi saxlanılır və axtarış bütün səviyyələrdə aparılır.

- 🔍 **Axtarış funksiyası**  
  Qovluq və fayllarda açar sözlə axtarış dəstəyi.

- 🕒 **Tarix yeniləmələri**  
  Hər dəyişiklik zamanı parent qovluqların `UpdateDate` sahəsi avtomatik yenilənir.

- 🧩 **Helper və xidmət strukturu**  
  Qovluq strukturu ilə işləmək üçün `FolderTreeHelper` və xidmət interfeysləri (IFolderService, IFolderFileService) ilə təmiz və genişləndirilə bilən memarlıq.

---

## 🧱 Layihə Strukturuna Baxış

### 📂 `Folder.Entities`
- **BaseFile.cs** – Fayl modelini (Id, SqlId, Name, CreateDate) təyin edir.  
- **FolderEntity.cs** – Qovluq strukturunu və daxilindəki fayl siyahısını saxlayır.  
  Hər qovluq `Children` (alt qovluqlar) və `Files` (fayllar) massivlərinə malikdir.

### 🧮 `Folder.HelperServices`
- **FolderTreeHelper.cs** –  
  - Qovluqları rekursiv tapır (`FindFolderRecursive`)  
  - Valideyn tarixlərini yeniləyir (`UpdateParentDates`)  
  - Qovluqları dərin klonlayır və yeni yola uyğunlaşdırır (`DeepCloneWithNewPath`)

### 🪣 `Folder.Roots`
- **RootFolders.cs** – Root ("/") səviyyəsini sabit dəyişən kimi saxlayır.

### 🧰 `Folder.Services.FolderServices`
- **FolderService.cs** – Əsas qovluq loqikasını idarə edir:
  - Qovluq yaratmaq (`CreateFolderAsync`)
  - Ad dəyişmək (`RenameFolderAsync`)
  - Kopyalama və köçürmə əməliyyatları (`BulkCopyFoldersAsync`, `BulkMoveFoldersAsync`)
  - Bir neçə mənbədən eyni anda silmə və köçürmə (`DeleteFromMultipleSourcesAsync`, `MoveFromMultipleSourcesAsync`)
  - İkon və şərh dəyişdirmə (`ChangeIconAsync`, `AddCommentAsync`)
  - Axtarış (`SearchInFolderAsync`)
- **IFolderService.cs** – `FolderService` üçün interfeys.

### 📄 `Folder.Services.FolderFileServices`
- **FolderFileService.cs** – Fayl səviyyəsində əməliyyatlar:
  - Fayl yaratmaq (`CreateFileAsync`)
  - Silmək və toplu silmək (`DeleteFileAsync`, `BulkDeleteFileAsync`)
  - Kopyalama və köçürmə (`CopyFilesAsync`, `MoveFilesAsync`)
  - Fayl adını dəyişmək (`RenameFileAsync`)
  - Qovluğa çoxlu fayl əlavə etmək (`AddFilesToFolderAsync`)
- **IFolderFileService.cs** – `FolderFileService` üçün interfeys.

---

## 🧩 Texnologiyalar

| Texnologiya | İstifadə məqsədi |
|--------------|------------------|
| **ASP.NET Core 8.0** | Backend API qurulması |
| **MongoDB.Driver** | Mongo verilənlər bazası ilə işləmək |
| **C# Record & Class Models** | Entity-lərin təsviri |
| **Helper Services** | Qovluq və fayl əməliyyatlarının rekursiv idarəsi |
| **SharedLibrary** | Exception və yardımçı metodların istifadəsi |
| **System.Text.Json** | Fayl və qovluq klonlama zamanı serialize/de-serialize əməliyyatları |

---
