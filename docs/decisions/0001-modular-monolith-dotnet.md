# ADR 0001 — Modüler monolit: ASP.NET Core 10 + Blazor Server + SQL Server, şirket içi IIS

- Status: Accepted
- Date: 2026-10-19

## Context
Polsan Leave Manager şirket içi, tek kurumlu bir izin uygulamasıdır: kişisel ve sağlık verisi
(KVKK) işler, kullanıcılar POLSAN Active Directory domain'indedir, ortam Windows ağırlıklıdır.
Ekip küçüktür; v1 hızlı ve denetlenebilir çıkmalıdır. Mevcut POLSAN Oracle verileriyle olası
entegrasyon ileride gündeme gelebilir.

## Decision
Tek ASP.NET Core 10 uygulaması (Blazor Server UI) olarak, modül başına ayrı şema/DbContext ve
`*.Contracts` arayüzleriyle sınırları testle zorlanan bir modüler monolit kuruyoruz; veri SQL
Server'da, deploy şirket içi Windows Server / IIS üzerinde, kimlik doğrulama Windows Auth.

## Consequences
- Kazanç: tek dil, tek deploy, tek toolchain; AD/Windows Auth ve IIS doğal; veri şirket içinde
  kaldığı için yurt dışı aktarım (KVKK) sorunu yok; modül sınırları ileride bölmeye izin verir.
- Bedel: Blazor Server her kullanıcı için kalıcı SignalR bağlantısı tutar (bu ölçekte sorun
  değil, ama yüksek gecikmeli VPN'de UX zayıflar); modül sınırları disiplin ve mimari test ister;
  Oracle entegrasyonu ayrı bir karar ve adapter gerektirir.

## Alternatives considered
- .NET API + React SPA: daha esnek UI, ama iki toolchain ve iki test yığını — v1 için gereksiz yük.
- Klasik katmanlı mimari: başta basit, ama iş kuralları dağılır ve modül sınırı test edilemez.
- Mikroservis: bu ölçekte operasyon yükü değerini aşar.
- Bulut barındırma: KVKK yurt dışı aktarım değerlendirmesi gerektirir; şirket içi altyapı mevcut.

## Revisit triggers
- Oracle / bordro entegrasyonu kapsama girerse (ayrı ADR).
- Kullanıcıların önemli kısmı uzaktan / yüksek gecikmeli bağlantıdan erişirse (Blazor Server UX).
- Bir modülün bağımsız ölçeklenme veya deploy ihtiyacı doğarsa.
