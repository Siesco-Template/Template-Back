# **Query Generator Library**

Bu kitabxana, **C\#** və **.NET** texnologiyalarından istifadə edilərək hazırlanmış, **Entity Framework Core (EF Core)** əsasında dinamik sorğular yaratmaq üçün nəzərdə tutulmuş bir həll yoludur. O, front-end tərəfindən gələn sorğu parametrlərinə (`columns`, `filters`, `sorting`, `pagination`) əsaslanaraq, back-end-də mürəkkəb və çevik sorğular formalaşdırmağı avtomatlaşdırır.

Layihənin əsas məqsədi, API endpoint-lərində hər bir sorğu növü üçün əl ilə kod yazmaq əvəzinə, vahid bir giriş nöqtəsi (`TableQueryRequest`) vasitəsilə dinamik sorğu qurulmasını təmin etməkdir. Bu, inkişaf prosesini sürətləndirir və kod təkrarını azaldır.

## 🚀 **Əsas Xüsusiyyətlər**

  - **Dinamik Sorğu Yaradılması:** Mərkəzi `TableQueryRequest` obyektinə əsaslanaraq, `columns`, `filters`, `sorting` və `pagination` parametrlərini dinamik olaraq birləşdirərək vahid bir sorğu yaradır.
  - **Seçilmiş Sütunlar (`Dynamic Projection`):** İstifadəçinin front-end-dən göndərdiyi sütun siyahısına uyğun olaraq, sorğu nəticələrini dinamik şəkildə formalaşdırır (`DynamicProjectionHelper`). Bu, yalnız tələb olunan məlumatların ötürülməsini təmin edərək performansın artmasına kömək edir.
  - **Avtomatik Filtrləmə:** Əvvəlki **Filter Service Library**-də yaradılan filtrləmə məntiqindən istifadə edərək, sorğuya dinamik filtrlər tətbiq edir. Həm ad-hoc filtrlər, həm də susmaya görə saxlanılan filtrlər avtomatik olaraq sorğuya əlavə olunur.
  - **Çevik Sıralama (`Sorting`):** Verilən sütun adı (`SortBy`) və sıralama istiqamətinə (`SortDirection`) əsasən sorğunun nəticələrini sıralayır.
  - **Verilənlər Bazasından Müstəqillik:** `EntitySetProvider` sinfi vasitəsilə `DbContext`-dən dinamik olaraq entity tiplərini və `IQueryable` obyektlərini əldə edir, bu da kodun verilənlər bazası modelindən asılılığını azaldır.

-----

## 🛠️ **Necə İstifadə Etmək Olar?**

Bu kitabxananın əsas istifadə məntiqi, front-end-dən `TableQueryRequest` obyektini qəbul edən bir controller endpoint-i yaratmaqdır.

#### **1. `TableQueryRequest` Obyektinin Quruluşu**

Aşağıda, front-end-dən göndərilməli olan sorğu obyektinin nümunəsi verilmişdir:

```csharp
public class TableQueryRequest
{
    public string TableId { get; set; } // Verilənlər bazası cədvəlinin adı
    public string Columns { get; set; } // Vergüllə ayrılmış sütun adları (məs: "Id, Name, Price")
    public List<FilterKeyValue> Filters { get; set; } // Filtrlər siyahısı
    public PaginationRequest? Pagination { get; set; } // Səhifələmə parametrləri
    public string? SortBy { get; set; } // Sıralanacaq sütunun adı
    public bool? SortDirection { get; set; } // Sıralama istiqaməti (true -> artan, false -> azalan)
}

public class FilterKeyValue
{
    public string Column { get; set; }
    public string Value { get; set; }
    public FilterOperationType FilterOperation { get; set; }
}
```

#### **2. Sorğunun Yaranması**

`TableQueryRequest` obyekti alındıqdan sonra, sorğunun necə yaradıldığını göstərən daxili məntiq:

```csharp
private async Task<IQueryable<dynamic>> GenerateQuery(TableQueryRequest tableRequest)
{
    // 1. Cədvəlin (Entity-nin) tipini əldə etmək
    var entityType = _setProvider.GetEntityType(tableRequest.TableId);

    // 2. IQueryable obyektini almaq
    var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);

    // 3. Filtrləri tətbiq etmək
    var filteredQuery = await _filterService.ApplyFilter(query, new FilterDto { Filters = tableRequest.Filters, TableId = tableRequest.TableId });

    // 4. Sıralamanı (Sorting) tətbiq etmək
    filteredQuery = filteredQuery.ApplySorting(entityType, tableRequest.SortBy, tableRequest.SortDirection);

    // 5. Seçilmiş sütunları yığmaq
    return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, tableRequest.Columns);
}
```

-----

## ⚙️ **Daxili Komponentlər**

| Komponent                  | Məqsəd                                                                                                                              |
|:---------------------------|:------------------------------------------------------------------------------------------------------------------------------------|
| **`EntitySetProvider`** | `DbContext` vasitəsilə dinamik olaraq cədvəl tiplərini tapır və `IQueryable` obyektləri təmin edir.                                    |
| **`FilterService`** | `TableQueryRequest` daxilində gələn filtrləri və ya susmaya görə filtrləri (`DefaultFilter`) sorğuya tətbiq edir.                       |
| **`DynamicProjectionHelper`**| Verilən sütun adlarına əsasən `IQueryable` obyektinə `Select` əməliyyatını dinamik şəkildə tətbiq edir.                               |
| **`ApplySorting`** | Verilmiş sütun və istiqamətə görə `IQueryable` obyektini sıralayır.                                                                  |

-----
