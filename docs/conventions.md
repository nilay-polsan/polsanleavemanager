# Conventions

> Bootstrap'ta karara bağlandı (2026-10-19). Buradaki her kural ya araçla zorlanır ya da review'da kontrol edilir.

## Language
- Chat language (interviews, sessions): Turkish (`tr`)
- Document language (docs/, specs, plans, ADRs, review and verify reports): Turkish (`tr`)
- Always English (protocol, not prose): ANEW core files, template headings and field labels,
  `Status:` values (Draft / Approved / In progress / Shipped), `Approved by / on:`, `Source:`.
- Code identifiers, branch names, commit messages: English. Domain terimlerinin kod karşılığı
  `docs/domain.md` tablosundadır.
- UI metinleri: Türkçe, `.resx` kaynak dosyalarında (kod içinde sabit metin yok).
- Agent messages (handoff summaries, refusals, `Next:` lines): chat language; command names, file
  paths and protocol fields verbatim.

## Language & framework versions
- .NET SDK 10.0.x (LTS) — `global.json` ile sabitlenir; C# `LangVersion` varsayılan, `Nullable` enable.
- ASP.NET Core 10, Blazor Server (Interactive Server render mode), EF Core 10 (SQL Server provider).
- SQL Server (üretim) · SQL Server LocalDB (geliştirme ve entegrasyon testleri).
- NuGet sürümleri merkezi: `Directory.Packages.props` (Central Package Management).

## Naming
- .NET standart isimlendirmesi (PascalCase tip/üye, `_camelCase` private alan, `I` önekli arayüz).
- Projeler: `PolsanLeaveManager.<Module>`, `PolsanLeaveManager.<Module>.Contracts`,
  testler `PolsanLeaveManager.<Module>.Tests`.
- Testler: `Method_State_ExpectedBehavior`; iş kuralı testi adında kural no geçer
  (`Approve_ByRequester_FailsWith_BR12`).
- Branch ve commit: `docs/git.md`.

## Error handling
- İş kuralı ihlali exception değildir: uygulama servisleri `Result` / `Result<T>` döner; hata
  `Error(Code, Message)` taşır, `Code` ilgili BR numarasıdır (`BR-9`).
- Beklenmeyen hatalar global exception handler'da yakalanır, correlation ID ile loglanır;
  kullanıcı Türkçe genel bir mesaj ve correlation ID görür.
- Kullanıcıya asla sızmaz: stack trace, SQL, bağlantı dizesi, iç tip adları, başka çalışanın
  verisi, sağlık verisi.

## Data rules
- İzin süresi: `decimal` (DB: `decimal(5,1)`), 0,5 katları; `float`/`double` yasak.
- İzin tarihleri: `DateOnly` (saatsiz). Denetim/olay zaman damgaları: `DateTimeOffset` UTC.
  Gösterim saat dilimi: `Europe/Istanbul`. "Bugün" `TimeProvider` üzerinden alınır, `DateTime.Now` yasak.
- ID'ler: `Guid` (sıralı üretim, `Guid.CreateVersion7()`).
- Kayıtlar fiziksel olarak silinmez; iptal/pasif durumuna alınır. Ledger ve denetim kaydı değişmez.
- Bakiye yalnızca `LeaveBalance` ledger hareketiyle değişir.

## Enforced by tooling
- Derleyici: `Nullable` enable, `TreatWarningsAsErrors` (`Directory.Build.props`).
- Analyzer'lar: .NET SDK analyzer'ları (`AnalysisLevel` latest-recommended) + `.editorconfig`.
- Format: `dotnet format --verify-no-changes`.
- Mimari sınırlar: NetArchTest testleri (`docs/architecture.md`, Forbidden dependencies).
- Bilinen açıklı paketler: `dotnet list package --vulnerable`.
- Hepsi `scripts/check` içinde (walking skeleton spec'i ile aktifleşir).
