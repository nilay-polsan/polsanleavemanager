# Security

> Bootstrap'ta karara bağlandı (2026-10-19). Ajanlar her plan ve review'da bu kurallara uyar.
> Uygulama kişisel veri ve özel nitelikli kişisel veri (sağlık) işler — KVKK kapsamındadır.

## Secrets
- Secrets never enter the repo, specs, prompts, or chat. `.env` is gitignored; provide `.env.example`.
- Agents never print secret values, even when debugging.
- Bağlantı dizeleri ve SMTP kimlik bilgileri: geliştirmede `dotnet user-secrets`, üretimde IIS
  ortam değişkeni / sunucu yapılandırması. `appsettings*.json` içinde secret yok.

## Input & output
- Doğrulama sınırda: her uygulama servisi girdisini doğrular (tarih aralığı, 0,5 gün katı, izin
  türü); UI doğrulaması yalnızca kullanıcı deneyimi içindir, güvenlik sağlamaz.
- Veri erişimi yalnızca EF Core ile, parametreli; ham SQL string birleştirme yasak.
- Blazor'un varsayılan HTML encoding'i kapatılmaz (`MarkupString` kullanıcı verisiyle yasak).
- Hata mesajları ve loglar: stack trace, SQL ve başka çalışanın verisi kullanıcıya çıkmaz;
  sağlık verisi (rapor tarihi/no, tanı) **hiçbir loga, e-postaya veya bildirime yazılmaz**.
- E-posta bildirimleri yalnızca talep durumu ve tarih içerir; rapor/doğum kaydı bildirim üretmez.

## AuthN / AuthZ
- Kimlik doğrulama: Windows Authentication (IIS, Negotiate/Kerberos). Uygulama şifre saklamaz.
  AD kullanıcı adı, İK'nın girdiği çalışan kartına eşlenir; eşlenmemiş kullanıcı hiçbir şey göremez.
- Roller: `Employee` (herkes), `Manager` (doğrudan bağlı çalışanı olan — ilişkiden türetilir),
  `HR` (ilk HR kullanıcısı kurulum yapılandırmasından tanımlanır; sonraki her HR atama/kaldırma
  yalnızca HR rolüyle, yetki kontrolünden geçerek yapılır ve denetim loguna yazılır — BR-22).
- **Default-deny:** açıkça izin verilmeyen her işlem reddedilir. Yetki her uygulama servisinde
  kontrol edilir; UI'da gizlemek yetki değildir.
- Employee: yalnızca kendi kayıtları. Manager: ek olarak doğrudan bağlı çalışanlarının talepleri.
  HR: tüm kayıtlar; rapor/doğum kayıtlarını **yalnızca HR** okur ve yazar.
- Kimse kendi talebini onaylayamaz (BR-12). Onay, ret, iptal, avans, düzeltme, rol değişikliği
  ve sağlık kaydı erişimi denetim loguna yazılır (BR-18).
- Rol yönetimi: yalnızca HR; kimse kendi rolünü değiştiremez (BR-33); son aktif HR'ın rolü
  kaldırılamaz, eşzamanlı işlemlerde de (BR-32, DB transaction ile). İlk HR tanımlama mekanizması
  yalnızca aktif HR yokken çalışır (BR-22) — yapılandırmadaki değer her açılışta yeniden uygulanmaz.
- Rol değişikliği bir sonraki yetki kontrolünde geçerlidir (BR-34): yetki, açık Blazor bağlantısındaki
  eski claim'lerden değil güncel rol bilgisinden okunur.
- İptal yetkisi: bekleyeni çalışan geri çeker; onaylı / başlamamış izin için çalışan yalnızca iptal
  talebi açar, kararı HR verir; başlamış izni yalnızca HR iptal eder / kısaltır (BR-26…BR-28).
- Onaylı talebin gün / bakiye düzeltmesi ve `SplitRuleMode` değişikliği yalnızca HR onayıyla
  (BR-21, BR-24).

## Dependencies
- Her yeni NuGet paketi planda gerekçesiyle önerilir; insan onaylamadan eklenmez.
- Kontroller: lisans MIT / Apache-2.0 / BSD (GPL/AGPL yasak); son 12 ayda bakım; bilinen
  CVE yok (`dotnet list package --vulnerable --include-transitive`, `scripts/check` içinde).
- Sürümler `Directory.Packages.props` ile merkezi; floating sürüm (`*`) yasak.

## Review lens
Security is a mandatory dimension of every independent review (see `prompts/review.md`), not a
separate afterthought phase.
