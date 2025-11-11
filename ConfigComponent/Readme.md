# ⚙️ Config Component – İstifadəçi və Sistem Konfiqurasiyası Xidməti

## 📘 Layihə Haqqında
**Config Component** layihəsi istifadəçilərin fərdi interfeys, cədvəl və sistem ayarlarını dinamik şəkildə idarə edən konfiqurasiya (configuration) sistemidir.  
Layihə **MongoDB** üzərində qurulub və **default (sistem)** və **user-specific (istifadəçi)** konfiqurasiyaları birləşdirərək tam konfiqurasiya obyektini formalaşdırır.

---

## ⚙️ Əsas Xüsusiyyətlər

- 🧩 **İstifadəçi əsaslı konfiqurasiya**  
  Hər istifadəçi öz fərdi ayarlarını saxlaya və dəyişdirə bilir (məsələn, cədvəl sütun düzülüşü, tema rejimi, görünən elementlər və s.)

- 🔄 **Default və User konfiqurasiyasının birləşdirilməsi**  
  `ConfigOverrideHelper` vasitəsilə `defaultConfig` və `userOverrides` obyektləri birləşdirilərək tam, son konfiqurasiya yaradılır.

- 🧠 **Ağıllı Override mexanizmi**  
  Override-lar `path` əsaslı tətbiq olunur.  
  Məsələn: `"theme.mode": "dark"` → `defaultConfig.theme.mode` dəyişdirilir.

- 🧮 **Cədvəl sütun sıralaması dəstəyi**  
  İstifadəçilərin fərdi `columnsOrder` dəyərlərinə əsasən sütunlar dinamik qaydada sıralanır.

- 🧰 **MongoDB əsaslı saxlanma**  
  Bütün konfiqurasiya sənədləri `Configs` kolleksiyasında saxlanılır.  
  Hər istifadəçi üçün ayrıca `userId` əsaslı sənəd mövcuddur.

- 🧱 **Sadə inteqrasiya üçün extension method**  
  `RegisterConfigComponent` metodu ilə istənilən .NET layihəsinə asanlıqla əlavə olunur.

---

## 🧱 Layihə Strukturuna Baxış

### 📂 `ConfigComponent.Dtos`
- **ConfigResponseDto.cs** –  
  Default və user konfiqurasiyalarını `JObject` formatında saxlayır (`DefaultConfig`, `UserConfig`).

### ⚙️ `ConfigComponent.Helpers`
- **ConfigOverrideHelper.cs** –  
  Override-ları `path` əsaslı olaraq `JObject` üzərində tətbiq edir.  
  - `ApplyOverridesToFullConfig()` – User dəyişikliklərini default konfiqurasiya üzərinə tətbiq edir.  
  - Dərin `JArray` və `JObject` strukturlarını dəstəkləyir.

### 🧩 `ConfigComponent.Services`
- **ConfigRegistration.cs** –  
  `.RegisterConfigComponent()` extension metodu vasitəsilə servisləri ASP.NET layihəsinə əlavə edir.  
  - `MongoDbService`  
  - `ConfigService`  
  - `MongoDbSettings`

- **ConfigService.cs** –  
  Əsas konfiqurasiya loqikasını təmin edir:
  - `GetDefaultAndUserConfigAsync()` → Default və user konfiqurasiyalarını gətirir.  
  - `CreateOrUpdateUserConfigAsync()` → Mövcud istifadəçinin fərdi ayarlarını əlavə və ya yeniləyir.  
  - `DeleteUserTableConfigAsync()` → İstifadəçinin müəyyən cədvəl üçün ayarlarını silir.  
  - Daxili metodlar:
    - `ApplyColumnOrdering()`  
    - `ApplyColumnOrderingWithOverrides()`  
    - `ApplyOrderingToAllTables()`

---

## 🧩 Texnologiyalar

| Texnologiya | İstifadə məqsədi |
|--------------|------------------|
| **ASP.NET Core 8.0** | Backend API |
| **MongoDB.Driver** | Verilənlər bazası inteqrasiyası |
| **Newtonsoft.Json (JObject)** | JSON strukturunun dinamik işlənməsi |
| **System.Text.Json** | Dəyərlərin çevrilməsi və serialize əməliyyatları |
| **SharedLibrary** | Ümumi servis və exception idarəetməsi |
| **Regex və LINQ** | Dərin `path` analizi və filter əməliyyatları |

---