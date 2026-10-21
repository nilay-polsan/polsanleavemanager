# Spec 0001 — Walking skeleton

- Status: Approved
- Mode: strict
- Plan: `specs/plans/0001-plan.md`

## Intent
Geliştirme ekibi, iş kurallarına başlamadan önce çalışan, derlenen, test edilen ve mimari
sınırları otomatik denetlenen boş bir uygulama iskeleti istiyor. Başarı: herkes tek komutla
(`./scripts/check`) iskeletin sağlıklı olduğunu görebilir ve uygulama açıldığında basit bir
Türkçe ana sayfa gösterilir. Bilerek yapılmayan: hiçbir iş kuralı (BR-n), veritabanı, kimlik
doğrulama, LocalDB entegrasyon testi, Playwright e2e ve kapsamlı CI. Bu spec TF-1 ve TF-2'yi
kısmen ilerletir; ikisini de kapatmaz (bkz. Constraints).

## Requirements
- R-1 Çözüm, `docs/architecture.md`'deki modül listesine birebir uyan bir proje yapısına sahiptir:
  8 modülün (`Personnel`, `LeaveRequests`, `LeaveBalance`, `Calendar`, `MedicalLeave`,
  `Notifications`, `Audit`, `Web`) her biri, `Web` hariç, bir iç proje ve bir `*.Contracts`
  projesiyle temsil edilir; isimler `docs/conventions.md` Naming kuralına uyar.
- R-2 Modüller iş kuralı ve veri içermez; yalnızca yapı ve bağımlılık sınırları vardır.
- R-3 `docs/architecture.md` Forbidden dependencies 1–4 otomatik testlerle denetlenir; testler
  boş bir kümeye karşı geçemez (denetlenen modül sayısı sıfırsa test başarısız olur).
- R-4 Uygulama yerel olarak başlatıldığında ana sayfa açılır ve Türkçe başlığı gösterir; metin
  kaynak dosyasından gelir (kod içinde sabit UI metni yok).
- R-5 Var olmayan bir adrese gidildiğinde Türkçe "sayfa bulunamadı" durumu gösterilir.
- R-6 `docs/conventions.md` "Enforced by tooling" maddeleri (SDK sabitleme, merkezi paket
  sürümleri, Nullable, uyarıların hata sayılması, analyzer'lar, format) çözüm genelinde etkindir.
- R-7 `./scripts/check` gerçek adımlar çalıştırır: restore, format, build, hızlı testler, açıklı
  paket taraması. LocalDB ve e2e adımları bu spec'te yorumda kalır.
- R-8 CI'daki `check` işi aynı `./scripts/check`'i çalıştırabilecek .NET 10 SDK'ya sahiptir
  (yalnızca SDK kurulumu; runner değişikliği ve LocalDB yok).
- R-9 TF-1 ve TF-2'nin durumu `docs/testing.md`'de gerçek sonuçlarla güncellenir; kapanış
  koşulları değişmez ve karşılanmayan koşul karşılanmış gösterilmez.

## User experience
- Flow: kullanıcı uygulamanın kök adresini açar → ana sayfayı görür. Var olmayan adres → "sayfa
  bulunamadı" sayfası ve ana sayfaya dönüş bağlantısı.
- States:
  - success — ana sayfa: uygulama adı ("Polsan İzin Yönetimi") başlık olarak.
  - not found — Türkçe bulunamadı mesajı + ana sayfa bağlantısı.
  - empty / loading / error / no permission — bu spec'te yok: sayfada veri yok, kimlik
    doğrulama yok; hata sayfası sonraki speclerde (bkz. Constraints).
- Design reference: none — the flow above is the design.
- Accessibility: sayfa `lang="tr"`; tek `h1`; bulunamadı sayfasındaki bağlantı klavyeyle
  (Tab + Enter) erişilebilir.

## Constraints & out of scope
- Out of scope: tüm BR-n; veritabanı / EF Core / migration; Windows Auth, roller, yetki;
  LocalDB ve `Integration` testleri; Playwright ve `E2E` testleri; global hata sayfası ve
  correlation ID; CI runner'ının Windows'a taşınması; OQ-5…OQ-8 ve V-1/V-2.
- TF-1 bu spec sonunda **açık kalır**: `test-integration` ve `test-e2e` adımları yorumda
  olduğundan kapanış koşulu ("adımlar aktif") karşılanmaz. TF-2 yalnızca iki residue profili
  de `RESIDUE CLEAN` çıkarsa ve kalan her süreç açıklanırsa kapanır; aksi halde açık kalır.
- Her yeni NuGet paketi planda gerekçesiyle listelenir ve insan onayı alır (`docs/security.md`).
- Kod yazımı bu spec'in ANALYZE segmentinde yapılmaz.

## Acceptance criteria
- [ ] AC-1 — Çözüm R-1'deki yapıya sahiptir: 7 modül × (iç proje + `*.Contracts`) + `Web`, ve
  test projeleri; proje adları `PolsanLeaveManager.<Module>[.Contracts|.Tests]` kalıbına uyar.
- [ ] AC-2 — `./scripts/check` (full) temiz bir klonda `CHECK GREEN (full, …)` ile biter ve
  restore, format, build, hızlı test ve açıklı paket adımlarını çalıştırır; build sıfır uyarıyla
  geçer (uyarı = hata).
- [ ] AC-3 — `./scripts/check --fast` `CHECK GREEN (fast, …)` ile biter.
- [ ] AC-4 — Her aktif test adımı 0'dan fazla test çalıştırır; sayı check çıktısında görünür.
- [ ] AC-5 — Forbidden dependencies 1–4'ün her biri için en az bir mimari test vardır ve geçer;
  denetlenen modül kümesi boşsa test başarısız olur.
- [ ] AC-6 — Kök adres başarı yanıtı döner ve sayfada "Polsan İzin Yönetimi" başlığı (tek `h1`,
  `lang="tr"`) bulunur; metin kaynak dosyasındadır.
- [ ] AC-7 — Var olmayan bir adres Türkçe bulunamadı mesajı ve ana sayfaya giden bağlantı gösterir.
- [ ] AC-8 — R-6'daki ayarlar etkindir; bir derleyici uyarısı build'i kırar, format ihlali
  `format` adımını kırar.
- [ ] AC-9 — CI `check` işi .NET 10 SDK kurar ve `./scripts/check`'i çalıştırır.
- [ ] AC-10 — `./scripts/residue` ve `./scripts/residue --fast` çalıştırılır; sonuçlar ve iki
  profilin süresi (en az 3 koşunun medyanı) raporlanır; `docs/testing.md` TF-1 ve TF-2 gerçek
  duruma göre güncellenir, kapanış koşulu karşılanmayan bulgu açık kalır.

## Definition of Done
- [ ] Every acceptance criterion mapped to proof (test or reproducible observation)
- [ ] `scripts/check` green
- [ ] Independent review done; real findings fixed, noise rejected with written rationale
- [ ] Docs / ADRs updated if behavior or architecture changed
- [ ] Spec moved to `specs/done/` (it becomes immutable there)

## Scorecard (fill at ship — honest numbers make the process improvable)
| Metric | Value |
|---|---|
| Spec revisions | |
| Fix rounds | |
| Review findings: real / noise | |
| Regressions introduced | |
| Bugs escaped to production | |
