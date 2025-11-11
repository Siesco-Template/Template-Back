# 📘 Permission Documentation

Bu sənəd `Permission.Api.Controllers.PermissionsController` daxilindəki endpoint-lərin istifadəsini, məqsədini və düzgün konfiqurasiya addımlarını izah edir. Sistem, microservice arxitekturasında istifadəçi icazələrini idarə etmək ücün hazırlanıb.

---

## 🔄 1. SyncUsers

**Route:** `POST /api/Permissions/SyncUsers`

**Məqsəd:**  
Sistemdə hal-hazırda mövcud olan bütün istifadəçiləri MongoDB-dəki `UserPermission` kolleksiyasına əlavə etmək.

**Qeyd:**  
Yeni istifadəçi əlavə olunduqda avtomatik olaraq bu servisdə event atılır və MongoDB-yə **icazələrsiz** qeyd əlavə olunur.  
Amma `Permission` servisi sonradan sistemə qoşulubsa, bu endpoint köhnə istifadəçiləri də Mongo kolleksiyasına əlavə etmək ücün istifadə olunur.

---

## 🔍 2. SyncAllMicroservicePagesAndActions

**Route:** `POST /api/Permissions/SyncAllMicroservicePagesAndActions`

**Məqsəd:**  
Bütün microservice-lərdən `Permission` attributları ilə işarələnmiş page və action-ları oxuyub toplayaraq MongoDB-yə qeyd edir.

### ⚙️ Addım-addım konfiqurasiya:

#### 1. Hədəf microservice-ə aşağıdakı controller əlavə edilməlidir:

```csharp
[Route("api/[controller]")]
[ApiController]
public class PermissionScannerController(PageAndActionScannerForServices _scanner) : ControllerBase
{
    [HttpPost("[action]")]
    public async Task<IActionResult> SyncPagesAndActions()
    {
        var controllerTypes = Assembly.GetEntryAssembly().GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        var pages = await _scanner.ScanPagesOnlyAsync(controllerTypes);
        return Ok(pages);
    }
}
```

#### 2. Permission servisində URL listinə yeni microservice-in endpoint-i əlavə olunmalıdır:

```csharp
var urls = new List<string>
{
    $"{_configuration["AuthService:BaseUrl"]}/PermissionScanner/SyncPagesAndActions",
    // digər servislərin URL-ləri də buraya əlavə olunur
};
```

###  Qeyd: Yeni Page və Action-ların Superadmin-ə Əlavə Edilməsi

Yeni page və action-lar sistemə əlavə edildikdə(SyncAllMicroservicePagesAndActions), onlar avtomatik olaraq superadmin istifadəçiyə icazə olaraq təyin olunur.

#### 📌 Nəzərə alınmalıdır:

- Sistemdə **rol əsaslı idarəetmə (role-based access)** tətbiq olunmadığı üçün, superadmin istifadəçi birbaşa `userId` vasitəsilə müəyyən edilir.
- Bu `userId` aşağıdakı kimi `SharedLibrary.StaticDatas.SuperAdmin` sinfində saxlanılır:

```csharp
public static class SuperAdmin
{
    public static readonly string SuperAdminUserId = "64f38b31c2628b2a541a194a";
}
```

- Əgər bu modul yeni bir layihəyə qoşulursa və ya superadmin istifadəçi dəyişdirilirsə:
  - `SuperAdminUserId` dəyəri **mütləq şəkildə yenilənmlidir**.
  - Əks halda, yeni icazələr səhv istifadəçiyə təyin oluna və ya superadmin icazələri verilməyə bilər.

---

## 🛠 3. UpdateUserPermissions

**Route:** `POST /api/Permissions/UpdateUserPermissions`

**Məqsəd:**  
Frontend-dən gələn permission siyahısına əsasən verilmiş user-in əvvəlki icazələri silinir və yenidən siyahı MongoDB-yə yazılır.

**Əsas Qeyd:**  
Bu endpoint-i çağıran istifadəçi yalnız **özündə mövcud olan icazələri** başqasına verə bilər.

---

## 📄 4. Permission Attributları Istifadə Qaydası

Kodda aşağıdakı kimi yazılır:

```csharp
[Permission(PageKeys.Permission, ActionKeys.GetAll)]
```

### ✅ Qaydalar:

- `PageKeys` və `ActionKeys` açarları SharedLibrary daxilində yazılmalıdır.
- Bu açarların Azərbaycan dilində qarşılığı `DisplayNames.resx` faylında saxlanılmalıdır ki, UI-də istifadəçiyə göstərilə bilsin.

---

## ℹ️ Digər Endpoint-lər

### `GET /api/Permissions/GetAllPagesAndActions`
> Sistemdə mövcud olan bütün page və action-ları MongoDB-dən qaytarır (cari istifadəçinin icazəsi olanları).

### `GET /api/Permissions/GetAllUserPermissions?skip=0&take=10`
> Bütün istifadəçilərin icazə siyahısını səhifələmə ilə qaytarır.

### `GET /api/Permissions/GetUserPermissionsById?userId=...`
> Verilmiş `userId`-yə sahib istifadəçinin icazələrini qaytarır.

### `GET /api/Permissions/GetCurrentUserPermissions`
> Hal-hazırkı login olmuş istifadəçinin icazələrini qaytarır.

### `GET /api/Permissions/CheckPermission?userId=...&page=...&action=...`
> Müəyyən bir istifadəçinin verilmiş page və action-a icazəsinin olub-olmadığını yoxlayır.
