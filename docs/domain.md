# Domain

> Bootstrap'ta karara bağlandı (2026-10-19). Kod İngilizce, konuşma ve dokümanlar Türkçe: her
> terimin kod karşılığı aşağıdadır — kodda başka isim kullanılmaz.

## Ubiquitous language

| Term | Meaning | Notes / not to be confused with |
|---|---|---|
| Çalışan (`Employee`) | İK'nın sisteme girdiği personel kartı | Kullanıcı (`User`) ≠ çalışan: kullanıcı AD kimliğidir, çalışana eşlenir |
| Yönetici (`Manager`) | Çalışanın doğrudan bağlı olduğu çalışan | Rol değil ilişkidir; Manager rolü bu ilişkiden türetilir |
| İK (`HR`) | İK rolündeki kullanıcı | |
| İzin talebi (`LeaveRequest`) | Çalışanın belirli tarih aralığı için açtığı talep | Rapor/doğum kaydı talep değildir |
| İzin türü (`LeaveType`) | `Annual`, `Excuse`, `Unpaid`, `Medical`, `Maternity` | |
| Yıllık izin (`AnnualLeave`) | Bakiyeden düşen ücretli izin | Tek bakiyeli tür |
| Mazeret izni (`ExcuseLeave`) | Kanuni mazeret izinleri (evlilik, ölüm, babalık…) | Bakiyeden düşmez |
| Ücretsiz izin (`UnpaidLeave`) | Ücretsiz izin | Bakiyeden düşmez |
| Rapor (`MedicalLeave`) | İK'nın girdiği hastalık raporu kaydı | Özel nitelikli kişisel veri; belge saklanmaz |
| Doğum izni (`MaternityLeave`) | İK'nın girdiği doğum izni kaydı | Babalık izni = mazeret izni |
| Kıdem (`Seniority`) | İşe giriş tarihinden itibaren tam yıl | |
| Hak ediş (`Accrual`) | Yıldönümünde bakiyeye eklenen yıllık izin günü | |
| Bakiye (`Balance`) | Ledger hareketlerinin toplamı | Saklanan bir alan değildir |
| Ledger hareketi (`BalanceEntry`) | Bakiyeyi değiştiren tek kayıt: hak ediş, kullanım, iade, avans, düzeltme | Silinmez, ters kayıtla düzeltilir |
| Avans (`Advance`) | İK onayıyla bakiyenin eksiye düşmesi | |
| Avans limiti (`AdvanceLimit`) | Eksi bakiyenin üst sınırı (sistem ayarı) | |
| İş günü (`WorkDay`) | Hafta sonu ve resmi tatil olmayan gün | Arife = yarım iş günü |
| Takvim yılı (`CalendarYear`) | Bir yılın tatil takvimi; `Draft` veya `Confirmed` (İK kesinleştirir) | Sabit tatil şablonu kesinleşmiş takvim değildir |
| Geçici hesap (`Provisional`) | Takvimi kesinleşmemiş güne dayanan gün sayısı | Takvim kesinleşince yeniden hesaplanır (BR-24) |
| Düzeltme önerisi (`RecalculationProposal`) | Onaylı talebin yeniden hesap sonucu; İK onayı bekler | Onaylanmadan ledger'a yazılmaz |
| Geri çekme (`Withdrawal`) | Bekleyen talebin çalışan tarafından geri alınması | İptal değildir |
| İptal talebi (`CancellationRequest`) | Onaylı, başlamamış izin için çalışanın İK'ya iptal başvurusu | İptalin kendisi değildir |
| Kısaltma (`Shortening`) | Başlamış iznin bitişinin İK tarafından öne çekilmesi | Orijinal aralık korunur |
| Hak ediş yılı (`AccrualYear`) | Bir yıldönümünde doğan hak ve ona ait dönem | Takvim yılı değildir |
| Tahsis (`Allocation`) | Kullanılan günün bir hak ediş yılına bağlanması (FIFO) | Hak edişten ayrı kayıt |
| Bölme kuralı modu (`SplitRuleMode`) | Bölme kontrolü başına `Off` / `Warn` / `Block` | Zorunlu kontrolde `Off` yok |
| Onay politikası (`ApprovalPolicy`) | Bir izin türü için talebi kimin onaylayacağını belirleyen kural | v1: tek politika (BR-11); türe göre yapılandırma ileride |
| Gün (`Days`) | İzin süresi, 0,5 katları | decimal, float değil |

## Business rules
- **BR-1** Yıllık izin hakkı, işe girişten itibaren 1 yıl tamamlanınca doğar ve her yıldönümünde
  yeniden hak edilir.
- **BR-2** Yıllık hak, yıldönümündeki kıdeme göre: 1–5 yıl (5 dahil) **14**, 5'ten fazla 15'ten az
  **20**, 15 ve üzeri **26** iş günü.
- **BR-3** Yıldönümünde 18 yaşından küçük veya 50 yaşından büyük çalışanın yıllık hakkı en az **20** gündür.
- **BR-4** İzin süresi yalnızca iş günü sayılarak hesaplanır; hafta sonu ve resmi tatiller
  düşülmez, arife günü 0,5 gün sayılır.
- **BR-5** İzin 0,5 gün katlarıyla talep edilebilir (yarım gün izin).
- **BR-6** Kullanılmayan yıllık izin yanmaz; bakiye yıllar arasında devreder.
- **BR-7** Yalnızca yıllık izin bakiyeden düşer; mazeret, ücretsiz, rapor ve doğum izni düşmez.
- **BR-8** Bekleyen (onay bekleyen) yıllık izin talepleri kullanılabilir bakiyeden düşülmüş sayılır.
- **BR-9** Kullanılabilir bakiyeyi aşan yıllık izin talebi yalnızca İK avans onayıyla
  onaylanabilir; avans sonrası bakiye `-AdvanceLimit`'in altına inemez.
- **BR-10** `AdvanceLimit` varsayılanı çalışanın bir sonraki yıllık hakkıdır; İK sistem ayarından değiştirebilir.
- **BR-11** Talebi çalışanın doğrudan yöneticisi onaylar veya reddeder; yöneticisi olmayan
  çalışanın talebini İK onaylar. v1'de bu, tüm talep edilebilir türler için tek onay politikasıdır;
  onay akışı izin türüne göre değiştirilebilir tasarlanır (`ApprovalPolicy`, `docs/architecture.md`).
- **BR-12** Kimse kendi talebini onaylayamaz veya reddedemez.
- **BR-13** Ret gerekçesi zorunludur.
- **BR-14** Rapor ve doğum izni kayıtlarını yalnızca İK girer; onay akışına girmez; belge
  saklanmaz (tarih aralığı + rapor no).
- **BR-15** Mazeret izni süreleri kanunidir: evlilik 3, evlat edinme 3, birinci derece yakın /
  eş / kardeş ölümü 3, eşin doğumu (babalık) 5 iş günü.
- **BR-16** Resmi tatil ve arife takvimini İK her yıl girer ve kesinleştirir. Takvimi kesinleşmemiş
  bir yıla ait tarihler için talep **engellenmez**: talep eden, onaylayan ve İK'ya "tatil takvimi
  kesinleşmedi — gün sayısı geçici" uyarısı gösterilir; İK'ya ayrıca kesinleşmemiş yıl için
  hatırlatma çıkar. Hesap kuralı: BR-23…BR-25.
- **BR-17** Aynı çalışanın tarih aralığı çakışan iki aktif (bekleyen veya onaylı) talebi / kaydı olamaz.
- **BR-18** Onay, ret, iptal, iptal talebi kararı, avans, bakiye düzeltmesi, yeniden hesaplama,
  rapor kaydı, bölme kuralı ayarı ve rol değişikliği denetim loguna yazılır.
- **BR-19** Mazeret, yıllık ve ücretsiz izin talebini çalışan kendisi oluşturur; rapor ve doğum
  izni kaydını yalnızca İK oluşturur (BR-14).
- **BR-20** İptal edilen iznin talebi, durum geçmişi ve iptal talebi silinmez: talep `Cancelled`
  durumuna geçer. Yıllık izinde iade, orijinal hareketleri değiştirmeden ters ledger hareketiyle
  yapılır; her iade hareketi talebe ve onu doğuran iptal / kısaltma kaydına bağlıdır (izlenebilir).
  İade tutarı, talebe bağlı tüm hareketlerin net toplamıdır (düzeltmeler dahil).
- **BR-21** Yıllık izin bölme kontrolleri (aynı hak ediş yılında üçten fazla parça; hiçbir parçanın
  en az 10 gün olmaması) parametrik yönetilir: kontrol başına `Off` / `Warn` / `Block`
  (`SplitRuleMode`). Ayarı yalnızca İK değiştirir (BR-18). Mevzuattan kaynaklanan zorunlu kurallar
  kurum tercihiyle devre dışı bırakılamaz: zorunlu olarak sınıflandırılan kontrol için `Off`
  seçilemez. Hangi kontrolün zorunlu olduğu, "10 gün"ün iş / takvim günü yorumu ve kesin eşikler
  mevzuat ve kurum uygulaması doğrulanarak belirlenir (V-2). İK'ya uygunluk raporu sağlanır
  (bölme koşullarını sağlamayan hak ediş yılları, çalışan bazında).
- **BR-22** İlk HR kullanıcısı kurulum yapılandırmasından tanımlanır; bu mekanizma **yalnızca
  sistemde aktif HR kullanıcısı yokken** çalışır ve denetim loguna yazılır. Sonraki her rol
  değişikliği (HR atama / kaldırma) yalnızca HR rolüyle yapılabilir, yetki kontrolünden geçer ve
  denetim loguna yazılır (BR-18).
- **BR-23** Takvimi kesinleşmemiş yıla düşen günler **geçici** hesaplanır (yalnızca hafta sonları
  düşülür) ve talep geçici olarak işaretlenir. Sabit tatil şablonu (her yıl aynı tarihli resmi
  tatiller) İK'nın takvim girişini kolaylaştırmak için kullanılabilir; ancak kesinleşmiş resmi
  takvim yerine geçmez — kesin hesabın tek dayanağı İK'nın kesinleştirdiği takvimdir.
- **BR-24** Takvim kesinleştiğinde veya kesinleşmiş takvim değiştiğinde, etkilenen talepler
  tespit edilir ve yeniden hesaplanır:
  - Bekleyen talepte hesap doğrudan güncellenir (bloke edilen bakiye dahil, BR-8).
  - Onaylı talepte gün sayısını veya bakiyeyi değiştiren her sonuç bir **düzeltme önerisi** olur
    ve İK onayına gider. İK onaylayana kadar mevcut onaylı kayıt ve ledger hareketleri olduğu gibi
    korunur; İK onaylayınca fark düzeltme ledger hareketiyle yazılır.
- **BR-25** Her yeniden hesaplamada önceki ve yeni değerler (gün sayısı, hesap dayanağı, takvim
  sürümü) ve düzeltme önerisinin İK kararı denetim loguna yazılır.
- **BR-26** Bekleyen talebi çalışan karar verilmeden önce geri çekebilir (`Withdrawn`); ledger
  hareketi oluşmaz, bloke edilen bakiye serbest kalır.
- **BR-27** Onaylı ama başlamamış izni çalışan doğrudan iptal edemez; **iptal talebi** oluşturur.
  İK iptal talebini onaylarsa izin iptal edilir (BR-20); reddederse izin onaylı kalır. İptal
  talebi sonuçlanana kadar izin onaylı ve bakiye etkisi aynen geçerlidir.
- **BR-28** Başlamış izni yalnızca İK iptal edebilir veya kısaltabilir; gerekçe zorunludur.
  Kısaltmada orijinal tarih aralığı korunur, kısaltma ayrı kayıttır; kullanılmayan günler ters
  ledger hareketiyle iade edilir (BR-20).
- **BR-29** "Başlamış izin" sınırı varsayılan olarak izin başlangıç günü saat 00:00'dır
  (Europe/Istanbul). Bu sınırın kurumun mesai / vardiya kurallarıyla uyumu uygulama öncesinde
  doğrulanır (V-1).
- **BR-30** Hak ediş ve tahsis ayrı modellenir: hak ediş hareketi bir **hak ediş yılı** tanımlar;
  kullanım, ayrı **tahsis** kayıtlarıyla hak ediş yıllarına dağıtılır.
- **BR-31** Yıllık izin kullanımı FIFO tahsis edilir: önce en eski hak ediş yılının kalan
  gününden düşülür. Kullanılan her günün hangi hak ediş yılına ait olduğu izlenebilir; iptal,
  kısaltma ve düzeltme tahsisi de ters kayıtla geri alır.
- **BR-32** Sistemdeki son aktif HR kullanıcısının HR rolü kaldırılamaz. Bu koruma eşzamanlı
  işlemlerde de geçerlidir ve veritabanı işlem bütünlüğüyle garanti edilir.
- **BR-33** Hiçbir kullanıcı kendi rolünü değiştiremez veya kaldıramaz.
- **BR-34** Rol değişikliği, etkilenen kullanıcının bir sonraki yetkilendirme kontrolünde geçerli
  olur (açık oturumlar dahil).

## Key domain invariants
- Bakiye = ledger hareketlerinin toplamı; hiçbir kod bakiyeyi doğrudan yazmaz.
- Ledger hareketi, tahsis kaydı ve denetim kaydı silinmez veya güncellenmez; hata ters kayıtla düzeltilir.
- Onaylı bir talep yalnızca iptal edilebilir veya kısaltılabilir; her ikisi bakiyeyi ters kayıtla iade eder.
- Onaylı bir talebin gün sayısı veya bakiye etkisi İK onayı olmadan değişmez.
- Sağlık verisi (rapor/doğum kaydı) HR rolü dışında hiçbir kullanıcıya, loga veya bildirime çıkmaz.
- Talep, durum geçmişi, iptal talebi ve ledger hareketi iptal veya düzeltme sebebiyle silinmez;
  düzeltme her zaman yeni bir kayıttır.
- Sistemde her an en az bir aktif HR kullanıcısı vardır (ilk kurulum anı hariç).

## Open questions
Karara bağlanmamış konular. Agent'ın önerisi yazılıysa insan onaylamadan kural olmaz.

- **OQ-5 — HR kullanıcısının kendi izin talebinin onayı.** BR-11 (yöneticisi olmayanın talebini
  HR onaylar) ve BR-12 (kimse kendi talebini onaylayamaz) birlikte, yöneticisi tanımlı olmayan
  tek HR kullanıcısının talebini onaylayacak kimse bırakmayabilir. HR kullanıcısının kendi
  talebini onaylayıp onaylayamayacağı ayrı bir iş kuralı olarak değerlendirilecek. **Karar yok.**
- **OQ-6 — Başlangıç anına kadar sonuçlanmamış iptal talebi (BR-27/BR-29).** İK kararından önce
  izin başlarsa iptal talebi ne olur (geçerli kalır mı, düşer mi, BR-28 kapsamına mı geçer)?
  **Karar yok.**
- **OQ-7 — FIFO tahsisinin kenar durumları (BR-21/BR-31).** Avansla kullanılan günün hangi hak
  ediş yılına tahsis edileceği; yarım gün kullanımın bölme kontrolünde ayrı parça sayılıp
  sayılmayacağı; birden çok hak ediş yılına yayılan tek talebin her yılda bir parça sayılıp
  sayılmayacağı. **Karar yok.**
- **OQ-8 — Son HR'ın dolaylı kaybı (BR-32).** Son aktif HR kullanıcısının çalışan kartının
  pasife alınması (işten çıkış) rol kaldırmayla aynı korumaya tabi mi? **Karar yok.**

## Pending validations
Kararı verilmiş, ama uygulama öncesinde dış doğrulama gerektiren noktalar.

- **V-1 — Başlamış izin sınırı (BR-29).** 00:00 varsayılanının mesai / vardiya kurallarıyla
  uyumu — sahibi: İK. Doğrulanana kadar varsayılan geçerlidir.
- **V-2 — İzin bölme mevzuatı (BR-21).** 4857 m.56 ve kurum uygulamasına göre: hangi bölme
  kontrolü zorunludur, "10 gün" yorumu, zorunlu kontrollerin en düşük izinli seviyesi —
  sahibi: İK + hukuk.
