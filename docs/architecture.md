# Architecture

> Bootstrap'ta karara bağlandı (2026-10-19). Ajanlar plan yapmadan önce bu dosyayı okur.

## System overview
Polsan Leave Manager, Polsan Teknoloji çalışanlarının izin talebi, yönetici onayı ve yıllık izin
bakiyesi (4857 sayılı İş Kanunu) için şirket içi bir web uygulamasıdır. Stil: **modüler monolit**
— tek ASP.NET Core 10 uygulaması (Blazor Server UI), tek SQL Server veritabanı, şirket içi
Windows Server / IIS üzerinde tek deploy. Gerekçe: bu ölçekte mikroservis operasyon yükü
gereksiz; sınırları testle zorlanan modüller ileride bölmeye izin verir (ADR-0001).

## Modules / components and ownership
Her modül kendi projesidir: `<Module>` (domain + application + infrastructure, `Internal`
namespace'i) ve `<Module>.Contracts` (dışarıya açık arayüzler, DTO'lar, domain event'leri).

| Module | Single responsibility | Owns |
|---|---|---|
| `Personnel` | Çalışan kartı, organizasyon, yönetici ilişkisi, rol ataması (İK girer) | Çalışan, işe giriş tarihi, doğum tarihi, yönetici, AD kullanıcı adı eşlemesi, rol atama geçmişi |
| `LeaveRequests` | Talep yaşam döngüsü: oluştur → onayla / reddet → geri çek / iptal talebi / iptal / kısalt; yeniden hesap | İzin talebi, durum geçmişi, iptal talepleri, kısaltmalar, düzeltme önerileri |
| `LeaveBalance` | Hak ediş, devir, avans, FIFO tahsis, bakiye, bölme kuralı değerlendirmesi | Bakiye hareketleri (ledger), hak ediş yılları, tahsis kayıtları, avans limiti ve `SplitRuleMode` ayarları |
| `Calendar` | Resmi tatil / arife takvimi, takvim kesinleştirme, iş günü hesabı | Takvim yılları (durum + sürüm), tatiller, sabit tatil şablonu |
| `MedicalLeave` | Rapor ve doğum izni kaydı (İK girer, onay akışı yok) | Rapor/doğum kaydı: tarih aralığı, rapor no — belge yok |
| `Notifications` | E-posta bildirimleri | Gönderim kayıtları |
| `Audit` | Denetim logu (kim, ne, ne zaman) | Denetim kayıtları |
| `Web` | Blazor Server UI + composition root; iş kuralı içermez | Hiçbir domain verisi |

## Communication rules
- Senkron ihtiyaç → hedef modülün `*.Contracts` arayüzü üzerinden doğrudan çağrı.
- Çapraz yan etki → in-process domain event (ör. `LeaveRequestApproved` → `LeaveBalance`
  ledger'a hareket yazar, `Notifications` e-posta, `Audit` kayıt). Event'in yayıncısı tüketiciyi
  bilmez.
- Tek veritabanı, **modül başına ayrı şema ve ayrı DbContext**; modüller arası JOIN, yabancı
  anahtar veya başka modülün tablosuna sorgu yasaktır — referans yalnızca ID ile.
- Bakiye her zaman ledger hareketlerinin toplamıdır; bakiye alanı doğrudan güncellenmez.
  İptal ve düzeltme ters/ek hareket olarak yazılır; mevcut hareket değiştirilmez (BR-20).
- Onay akışı `LeaveRequests` içinde `IApprovalPolicy` arkasındadır ve izin türünden politikaya
  bir eşleme ile seçilir. v1'de tüm talep edilebilir türler tek politikaya eşlenir (BR-11);
  türe göre farklı / çok kademeli politika eklemek mevcut talep akışını değiştirmeden yeni bir
  politika ve eşleme satırı eklemektir.
- Rol ataması `Personnel`'dedir; her rol değişikliği `Audit`'e event olarak gider (BR-22).
  Rol atamaları üzerine yazılmaz, geçmiş tutulur. Son aktif HR koruması (BR-32) aynı
  transaction içinde kilitli okuma + yazma ile yapılır; uygulama katmanındaki "önce say, sonra
  kaldır" kontrolü tek başına yeterli sayılmaz.
- Yetkilendirme her uygulama servisi çağrısında **güncel rol bilgisiyle** yapılır (BR-34);
  Blazor Server bağlantısı açıkken bellekte kalan oturum açılışı claim'lerine güvenilmez.
- Takvim ve yeniden hesap (BR-23…BR-25): `Calendar.Contracts.IWorkDayCalculator` gün sayısını
  hesap dayanağıyla (kesin / geçici, takvim sürümü) döner. `CalendarYearConfirmed` ve
  `CalendarYearChanged` → `LeaveRequests` etkilenen talepleri bulur: bekleyende hesabı günceller,
  onaylıda `RecalculationProposal` oluşturur. İK onayı → `RecalculationApproved` → `LeaveBalance`
  düzeltme hareketi + tahsis düzeltmesi; her adım `Audit`'e. Handler'lar idempotenttir (aynı
  talep + takvim sürümü için ikinci kez düzeltme yazılmaz).
- İptal (BR-26…BR-29): geri çekme ve iptal talebi `LeaveRequests` içinde durum geçişleridir.
  İptal / kısaltma kesinleşince `LeaveRequestCancelled` / `LeaveRequestShortened` → `LeaveBalance`
  talebe bağlı net tutarın tersini ve tahsis geri alımını yazar. İptal kuralları `IApprovalPolicy`
  ile aynı uzatma noktasındadır (tür bazında yapılandırılabilir; v1'de tek politika).
  "Başlamış" kararı `TimeProvider` ve Europe/Istanbul ile verilir.
- Hak ediş / tahsis (BR-30, BR-31): hak ediş hareketleri hak ediş yılını taşır; kullanım
  hareketiyle aynı transaction'da FIFO tahsis kayıtları yazılır. Bakiye yine hareketlerin toplamıdır.
- Bölme kuralı (BR-21): `LeaveBalance.Contracts.ISplitRuleEvaluator` tahsis bilgisini kullanarak
  uyarı / engel sonucunu döner; `LeaveRequests` talep oluşturma ve onayda çağırır. Kontroller
  zorunlu / kurum tercihi olarak sınıflandırılır; zorunlu kontrolde `Off` kabul edilmez.

## Forbidden dependencies (make them testable)
Her biri ArchUnitNET mimari testi olarak `scripts/check` içinde zorlanır.
1. Hiçbir modül başka bir modülün `Internal` namespace'ine veya DbContext'ine referans vermez —
   yalnızca `*.Contracts`.
2. `*.Contracts` projeleri hiçbir modül iç projesine, EF Core'a veya ASP.NET'e bağımlı değildir.
3. `Web` projesi EF Core'a, `DbContext`'e ve herhangi bir `Internal` tipe referans vermez.
4. Domain tipleri (`*.Internal.Domain`) EF Core, ASP.NET Core ve `System.Net.Http`'ye bağımlı değildir.
5. `MedicalLeave` verisine erişen her uygulama servisi HR rol kontrolünden geçer (yetki testi).

## Deliberately out of scope
v1 dışı: bordro entegrasyonu, Oracle / dış HR sistemi entegrasyonu (ayrı ADR ile değerlendirilecek;
`Personnel` arkasına adapter olarak gelir), çok kademeli onay ve türe göre farklı onay politikası
(tasarım hazır, kullanım ileride), vekalet, mobil uygulama, rapor belgesi saklama, harici tatil API'si.

## Deferred to their stage
- **SMTP / Exchange** (e-posta altyapısı, gönderici hesabı, kimlik doğrulama): `Notifications`
  spec'inde netleşir. O zamana kadar `Notifications` bir arayüz arkasında, test ortamında sahte gönderici.
- **Oracle entegrasyonu**: kapsama girdiği aşamada `/adr` ile karar.
