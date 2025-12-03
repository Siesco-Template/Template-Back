# ImportExportComponent Geliştirme Planı

## Genel Bakış
Bu dokümanda ImportExportComponent'in görsellere uygun olarak geliştirilmesi için yapılacaklar listelenmiştir.

## Yapılacaklar

### 1. Backend Geliştirmeleri

#### 1.1. Table Column Metadata Endpoint'i Ekleme
- **Dosya**: `Main/MainProject.API/Controllers/ExcelController.cs`
- **Açıklama**: Table column'larını döndüren yeni bir endpoint eklenmeli
- **Endpoint**: `GET /api/Excel/GetTableColumns?tableName={tableName}`
- **Response**: Table column'larının listesi (isim, tip, required, vb.)
- **Kullanım**: Frontend'de table column'larını göstermek için

#### 1.2. ValidateRequestDto Güncelleme
- **Dosya**: `ImportExportComponent/Dtos/ValidateRequestDto.cs`
- **Açıklama**: ValidateRequestDto'ya `TableName` zaten var, ancak mapping'lerin doğru şekilde gönderildiğinden emin olunmalı
- **Not**: Mevcut yapı uygun görünüyor

#### 1.3. ImportConfirmRequestDto Güncelleme
- **Dosya**: `ImportExportComponent/Dtos/ImportConfirmRequestDto.cs`
- **Açıklama**: ValidatedRecordDto'lardan sadece valid olanları almak için güncelleme gerekebilir
- **Not**: Mevcut yapı kontrol edilmeli

### 2. Frontend Geliştirmeleri

#### 2.1. Excel Dosya Yükleme Ekranı (İlk Fotoğraf)
- **Özellikler**:
  - Drag & drop alanı
  - Dosya seçme butonu ("Kompüterdən əlavə et")
  - Sadece .xlsx dosyaları kabul etme
  - Maksimum 1 dosya yükleme
  - Yüklenen dosyanın gösterilmesi (isim, boyut)
  - Dosya silme özelliği
  - "Ləğv et" ve "Təsdiqlə" butonları

- **API Entegrasyonu**:
  - `POST /api/Excel/UploadFile` - Excel dosyasını yükle ve preview al

#### 2.2. Column Mapping Ekranı (İkinci Fotoğraf)
- **Özellikler**:
  - Excel column'larının üstte gösterilmesi
  - Table column'larının altta gösterilmesi
  - Her table column için dropdown (Excel column seçimi)
  - Otomatik matching: İsimleri uygun gelen column'lar otomatik eşleştirilmeli
  - "Yeni fayl yüklə" butonu
  - Excel dosyasından gelen verilerin tablo formatında gösterilmesi
  - "Ləğv et" ve "Yoxla" butonları

- **API Entegrasyonu**:
  - `GET /api/Excel/GetTableColumns?tableName={tableName}` - Table column'larını al
  - Matching algoritması: Excel column isimleri ile table column isimlerini karşılaştır

#### 2.3. Validation Sonuçları Ekranı (Üçüncü Fotoğraf)
- **Özellikler**:
  - Validation sonuçlarının gösterilmesi
  - Hatalı hücrelerin kırmızı ile işaretlenmesi
  - Hata mesajlarının gösterilmesi
  - "Yadda saxla" butonu (sadece tüm validasyonlar geçildiğinde aktif)
  - "Ləğv et" butonu

- **API Entegrasyonu**:
  - `POST /api/Excel/Validate` - Verileri validate et
  - `POST /api/Excel/Save` - Valid verileri database'e kaydet

### 3. Otomatik Matching Algoritması

#### 3.1. Matching Kuralları
- **Tam eşleşme**: Excel column ismi ile table column ismi aynı ise
- **Case-insensitive eşleşme**: Büyük/küçük harf farkı göz ardı edilmeli
- **Türkçe karakter normalizasyonu**: ı, İ, ş, Ş, ğ, Ğ, ü, Ü, ö, Ö, ç, Ç karakterleri normalize edilmeli
- **Boşluk ve özel karakter temizleme**: Boşluklar, tire, alt çizgi gibi karakterler temizlenmeli
- **Kısmi eşleşme**: Eğer tam eşleşme yoksa, benzerlik skoru yüksek olanlar eşleştirilmeli

#### 3.2. Matching Önceliği
1. Tam eşleşme (case-insensitive)
2. Normalize edilmiş eşleşme
3. Kısmi eşleşme (benzerlik skoru > 0.8)

### 4. Validation Hatalarının Gösterilmesi

#### 4.1. Hata İşaretleme
- Hatalı hücreler kırmızı arka plan ile işaretlenmeli
- Hover durumunda hata mesajı gösterilmeli
- Hatalı satırlar belirgin şekilde işaretlenmeli

#### 4.2. Hata Mesajları
- Her hata için açıklayıcı mesaj gösterilmeli
- Türkçe/Azerbaycan Türkçesi dilinde mesajlar
- Field bazında hata mesajları

### 5. State Management

#### 5.1. Component State'leri
- `uploadedFile`: Yüklenen Excel dosyası bilgisi
- `excelData`: Excel'den gelen preview verileri
- `tableColumns`: Database table column'ları
- `columnMappings`: Excel column -> Table column mapping'leri
- `validationResults`: Validation sonuçları
- `isValidating`: Validation işlemi devam ediyor mu?
- `isSaving`: Kaydetme işlemi devam ediyor mu?

#### 5.2. Step Management
- Step 1: Dosya yükleme
- Step 2: Column mapping
- Step 3: Validation ve kaydetme

### 6. UI/UX İyileştirmeleri

#### 6.1. Loading States
- Dosya yükleme sırasında loading göstergesi
- Validation sırasında loading göstergesi
- Kaydetme sırasında loading göstergesi

#### 6.2. Error Handling
- API hatalarının kullanıcıya gösterilmesi
- Network hatalarının yakalanması
- Validation hatalarının kullanıcı dostu gösterilmesi

#### 6.3. Success Messages
- Başarılı kaydetme mesajı
- Kaç kayıt eklendiği bilgisi

### 7. Test Senaryoları

#### 7.1. Dosya Yükleme Testleri
- Geçerli .xlsx dosyası yükleme
- Geçersiz dosya formatı kontrolü
- Birden fazla dosya yükleme kontrolü
- Büyük dosya yükleme kontrolü

#### 7.2. Mapping Testleri
- Otomatik matching testleri
- Manuel mapping testleri
- Mapping validation testleri

#### 7.3. Validation Testleri
- Required field validation
- Data type validation
- Foreign key validation
- Max length validation

#### 7.4. Save Testleri
- Valid verilerin kaydedilmesi
- Invalid verilerin filtrelenmesi
- Transaction rollback testleri

## Notlar

- Backend API'ler mevcut görünüyor, sadece table columns endpoint'i eklenmeli
- Frontend component'i oluşturulmalı veya güncellenmeli
- Matching algoritması frontend'de implement edilmeli
- Validation hatalarının görsel gösterimi frontend'de yapılmalı
- Tüm işlemler async olmalı ve loading state'leri gösterilmeli

