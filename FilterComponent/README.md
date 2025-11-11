# **Filter Service Library**

Bu kitabxana **C\#** və **.NET** texnologiyalarından istifadə edilərək hazırlanmışdır və tətbiqlərinizə güclü və çevik məlumat filtrləmə imkanları əlavə etmək üçün nəzərdə tutulub. Xüsusilə **MsSQL** ilə inteqrasiya üçün optimallaşdırılmışdır. Kitabxana, back-end tərəfində hər `get` endpoint-i üçün təkrar yazılan filtrləmə kodlarını aradan qaldırır.

Əsas məqsədi, istifadəçilərin dinamik olaraq filtrləmə meyarları yaratmasına, bu meyarları saxlamağına və istənilən `IQueryable<T>` mənbəyinə asanlıqla tətbiq etməsinə imkan verməkdir. Bu, xüsusilə mürəkkəb filtrləmə ehtiyacları olan tətbiqlər üçün vaxta qənaət edən və effektiv bir həll yoludur.

-----

## 🚀 **Əsas Xüsusiyyətlər**

  - **Dinamik Sorğu Yaradılması:** Kitabxana, istifadəçi tərəfindən müəyyən edilmiş filtrləri **MsSQL-ə xas dinamik sorğulara** (`IQueryableExtensions.GenerateQuery`) çevirir. Bu, hər filtr üçün əl ilə kod yazmaq əvəzinə, məntiqi dinamik olaraq qurmağa imkan verir.
  - **Saxlanılan Filtrlər (`Saved Filters`):** İstifadəçilər tez-tez istifadə etdikləri filtrləmə parametrlərini yadda saxlaya və daha sonra təkrar istifadə edə bilərlər. Bu, fərdiləşdirilmiş məlumat görünüşləri yaradaraq istifadəçi təcrübəsini zənginləşdirir.
  - **Susmaya görə Filtr İdarəetməsi:** Hər istifadəçi müəyyən bir cədvəl üçün bir filtri **susmaya görə (default)** təyin edə bilər. Bu filtr, istifadəçi müvafiq səhifəyə hər daxil olanda avtomatik tətbiq olunur. Bu funksionallıq `SetDefault` və `RemoveDefault` metodları ilə idarə olunur.
  - **CRUD Əməliyyatları:** Kitabxana, saxlanılan filtrlər üzərində tam nəzarət təmin edir. Siz `SaveFilter`, `GetFilterById`, `GetAllFilters`, `UpdateFilter` və `DeleteFilter` kimi metodlarla filtrləri asanlıqla idarə edə bilərsiniz.
  - **Çevik İstifadə:** `ApplyFilter` metodu sayəsində həm saxlanılan, həm də anlıq filtrlər istənilən `IQueryable<T>` mənbəyinə dinamik şəkildə tətbiq edilə bilər.

-----

## 🛠️ **Necə İstifadə Etmək Olar?**

Aşağıdakı kod fraqmentləri, kitabxananın əsas funksiyalarını necə istifadə edəcəyinizi göstərir.

#### **1. Yeni Bir Filtr Yaratmaq**

Bu nümunə, `CreateFilter` endpoint-i vasitəsilə yeni bir filtrin necə yaradıldığını göstərir. Burada **`price`** dəyərinin `100`-dən böyük olmasını yoxlayan bir filtr yaradılır.

```csharp
public class CreateFilterDto
{
    public string FilterTitle { get; set; }
    public string TableId { get; set; }
    public List<FilterKeyValue> FilterValues { get; set; }
}

var newFilterData = new CreateFilterDto
{
    FilterTitle = "Qiyməti 100-dən yuxarı olan məhsullar",
    TableId = "productsTable",
    FilterValues = new List<FilterKeyValue>
    {
        new FilterKeyValue
        {
            Column = "price",
            Value = "100",
            FilterOperation = FilterOperationType.GreaterThan // GreaterThan tipi istifadə edilir
        }
    }
};

await _filterService.SaveFilter(newFilterData);
```

#### **2. Susmaya Görə Filtrləri İdarə Etmək**

Bu kod parçası, bir filtri **susmaya görə** təyin etmə və ya ləğv etmə əməliyyatlarını göstərir.

```csharp
// Bir filtri susmaya görə təyin etmək
string filterIdToSetDefault = "60c72b2f9b1d7d001f8e4b7c";
await _filterService.SetDefault(filterIdToSetDefault);

// Susmaya görə ayarı ləğv etmək
string filterIdToRemoveDefault = "60c72b2f9b1d7d001f8e4b7c";
await _filterService.RemoveDefault(filterIdToRemoveDefault);
```

-----

## ⚙️ **`FilterOperationType` Enum-unun İzahı**

`FilterOperationType` enum-u, filtrləmə əməliyyatının növünü müəyyən edir. Bu, dinamik sorğuların düzgün qurulması üçün kritik əhəmiyyət daşıyır. Hər bir dəyərin funksionallığı aşağıda açıqlanır:

| Dəyər                  | Açıqlama                                                                    |
|:-----------------------|:----------------------------------------------------------------------------|
| `Equal`                | Dəyərin sütun dəyərinə **bərabər** olub-olmadığını yoxlayır.                  |
| `NotEqual`             | Dəyərin sütun dəyərinə **bərabər olmadığını** yoxlayır.                         |
| `Like`                 | Sütun dəyərinin verilən dəyəri **ehtiva edib-etmədiyini** yoxlayır (`Contains`). |
| `NotLike`              | Sütun dəyərinin verilən dəyəri **ehtiva etmədiyini** yoxlayır.                 |
| `GreaterThan`          | Sütun dəyərinin verilən dəyərdən **böyük** olduğunu yoxlayır.                    |
| `LessThan`             | Sütun dəyərinin verilən dəyərdən **kiçik** olduğunu yoxlayır.                    |
| `GreaterThanOrEqual`   | Sütun dəyərinin verilən dəyərə **bərabər və ya ondan böyük** olduğunu yoxlayır.   |
| `LessThanOrEqual`      | Sütun dəyərinin verilən dəyərə **bərabər və ya ondan kiçik** olduğunu yoxlayır.   |
| `In`                   | Sütun dəyərinin verilən dəyərlər siyahısına **daxil** olub-olmadığını yoxlayır. |
| `NotIn`                | Sütun dəyərinin verilən dəyərlər siyahısına **daxil olmadığını** yoxlayır.       |
| `RangeNumberOrDate`    | Sütun dəyərinin vergüllə ayrılmış **aralıqda** (`başlanğıc,son`) olduğunu yoxlayır. |

-----

## 📚 **API İstinadı**

Bu kitabxana tərəfindən təmin olunan əsas API endpoint-ləri aşağıda verilmişdir.

| Metod   | Endpoint                                  | Məqsəd                                                              |
|:--------|:------------------------------------------|:--------------------------------------------------------------------|
| `POST`  | `/api/Filter/CreateFilter`                | Yeni bir filtr yaradır.                                             |
| `GET`   | `/api/Filter/GetFiltersByTableId`         | Müəyyən `tableId`-yə aid bütün filtrləri gətirir.                  |
| `GET`   | `/api/Filter/GetFilterById`               | Verilmiş `filterId`-yə uyğun filtri gətirir.                       |
| `GET`   | `/api/Filter/GetDefaultFilter`            | Verilmiş `tableId` üçün cari istifadəçinin susmaya görə filtrini gətirir. |
| `PUT`   | `/api/Filter/SetDefaultFilter`            | Müəyyən filtri susmaya görə təyin edir.                             |
| `PUT`   | `/api/Filter/RemoveDefaultFilter`         | Susmaya görə təyin edilmiş filtri ləğv edir.                       |
| `PUT`   | `/api/Filter/UpdateFilter`                | Mövcud bir filtrin başlığını və tərkibini yeniləyir.               |
| `DELETE`| `/api/Filter/DeleteFilter`                | Müəyyən bir filtri silir.                                          |

-----

### **Layihəyə İnteqrasiya**

Bu kitabxananı istənilən .NET layihəsinə asanlıqla əlavə etmək mümkündür. İnteqrasiya üçün iki əsas addım mövcuddur: `appsettings.json` faylını konfiqurasiya etmək və `Program.cs` faylında xidməti qeydiyyatdan keçirmək.

#### **1. `appsettings.json` Konfiqurasiyası**

Əvvəlcə, layihənizin `appsettings.json` faylına aşağıdakı konfiqurasiya blokunu əlavə edin. Bu, kitabxananın MongoDB-yə qoşulması üçün lazımi məlumatları saxlayır.

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://Test:Test",
    "DatabaseName": "FilterDb"
  }
}
```

  - `ConnectionString`: MongoDB verilənlər bazasına qoşulma sətiri.
  - `DatabaseName`: Verilənlərin saxlanılacağı MongoDB verilənlər bazasının adı.

#### **2. `Program.cs` faylında Xidməti Qeydiyyatdan Keçirmək**

Növbəti addım olaraq, kitabxananın xidmətlərini layihənizin `Program.cs` faylında qeydiyyatdan keçirməlisiniz. Bu, `builder.RegisterFilterComponent()` metodunun köməyi ilə edilir.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Digər xidmətlərin qeydiyyatı ...

// Filter Service Library-nin qeydiyyatı
builder.RegisterFilterComponent("MongoDB");

// Əlavə konfiqurasiya və ya xidmət qeydiyyatları
// ...

var app = builder.Build();

// ...
```

## 🤝 **Layihəyə Dəstək**

Layihənin inkişafına dəstək olmaq istəyirsinizsə, lütfən aşağıdakı addımları izləyin:

1.  Layihəni Fork edin.
2.  Yeni bir Branch yaradın: `git checkout -b feature/YeniXususiyyet`
3.  Dəyişikliklərinizi edin və commit-ləyin: `git commit -m 'Yeni xüsusiyyət: ...'`
4.  Branch-ı push edin: `git push origin feature/YeniXususiyyet`
5.  Bir Pull Request (PR) açın.
