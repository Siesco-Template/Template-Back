# 🛡️ AuthService – İstifadəçi Autentifikasiya və İcazə Sistemi

## 📘 Layihə Haqqında
**AuthService** ASP.NET Core əsaslı autentifikasiya və icazə (authentication & authorization) xidmətidir.  
Layihə mikrosxidmət (microservice) memarlığında hazırlanmışdır və digər sistemlər üçün mərkəzi identifikasiya servisi kimi işləyir.

Bu xidmət istifadəçilərin qeydiyyatı, giriş/çıxışı, parol yeniləməsi, bloklanması və səlahiyyətlərinin idarə olunması funksiyalarını təmin edir.

---

## ⚙️ Əsas Xüsusiyyətlər

- 🔐 **İstifadəçi qeydiyyatı və giriş**  
  E-poçt və şifrə ilə giriş, həmçinin Refresh Token mexanizmi ilə sessiya yeniləmə dəstəyi.

- 🔑 **Parol idarəetməsi**  
  - Şifrə sıfırlama e-poçt vasitəsilə  
  - Admin tərəfindən parol dəyişmə  
  - Parol gücünün yoxlanması

- 🧾 **Bloklama funksiyası**  
  Admin tərəfindən istifadəçi bloklana və blok səbəbi + tarix qeyd edilə bilər.

- 📤 **Email göndəriş sistemi**  
  Parol sıfırlama və qeydiyyat bildirişləri üçün EmailService inteqrasiyası.

- 🧩 **Permission Scanner**  
  Controller-lərdəki `[Permission]` atributlarını avtomatik analiz edərək səhifə və əməliyyat siyahısını çıxarır.

- 🪶 **Event Publisher**  
  Yeni istifadəçi yaradıldıqda və ya qeydiyyatdan keçdikdə digər servislərə məlumat ötürülür (MassTransit / RabbitMQ vasitəsilə).

---

## 🧱 Layihə Strukturuna Baxış

### 📂 `Auth.Core/Entities`
- **AppUser.cs** – İstifadəçi məlumatları (Ad, Email, Şifrə, Rol, Token və s.)
- **LoginLog.cs** – Giriş cəhdlərinin tarixçəsi (müvəffəqiyyətli / uğursuz girişlər)
- **PasswordToken.cs** – Şifrə sıfırlama üçün yaradılan token məlumatı

### 📂 `Auth.Business/Services`
- **AuthService.cs** – Layihənin əsas loqikasını daşıyan servis.  
  Burada qeydiyyat, giriş, şifrə dəyişmə, token yeniləmə, istifadəçi bloklama və profil əməliyyatları həyata keçirilir.

### 📂 `Auth.Business/Helpers`
- **CurrentUser.cs** – Hal-hazırkı istifadəçinin məlumatlarını (ID, Rol, Ad Soyad) HTTP kontekstdən oxuyur.

### 📂 `Auth.API/Controllers`
- **AuthController.cs** – İstifadəçi autentifikasiyası və idarəetmə əməliyyatları üçün API endpoint-lər.  
- **UserController.cs** – Cədvəl əsaslı istifadəçi siyahılarını gətirir.  
- **PermissionScannerController.cs** – Controller-ləri analiz edərək sistemdəki səhifə və əməliyyatları çıxarır.

---

## 🧩 Texnologiyalar

| Texnologiya | İstifadə məqsədi |
|--------------|------------------|
| **ASP.NET Core 8.0** | API qurulması |
| **Entity Framework Core** | ORM və verilənlər bazası əməliyyatları |
| **MassTransit** | Event-driven kommunikasiya |
| **RabbitMQ** | Servislərarası mesaj ötürülməsi |
| **JWT (JSON Web Token)** | Token əsaslı autentifikasiya |
| **Microsoft IdentityModel** | Token oxuma və doğrulama |
| **EmailService** | Bildiriş e-poçtlarının göndərişi |
| **TableComponent** | Cədvəl əsaslı sorğu generatoru və nəticə gətirici helper |

---
