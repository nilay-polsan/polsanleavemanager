# ADR 0002 — Entegrasyon testleri SQL Server LocalDB'ye karşı

- Status: Accepted
- Date: 2026-10-19

## Context
Entegrasyon testleri gerçek SQL Server davranışını (transaction, constraint, decimal hassasiyeti)
doğrulamalıdır; EF InMemory bunu sağlamaz. Bootstrap sırasında geliştirme makinesinde Docker
kurulu ama engine çalışmıyordu; SQL Server LocalDB (2025) ise kuruluydu.

## Decision
Entegrasyon ve e2e testleri adanmış `PolsanLeaveTests` LocalDB örneğine karşı koşar;
Testcontainers kullanılmaz. CI `check` işi Windows runner'da çalışır.

## Consequences
- Kazanç: Docker bağımlılığı yok; geliştirici makinelerinde LocalDB zaten var; gerçek SQL Server motoru.
- Bedel: testler yalnızca Windows'ta koşar; CI Windows runner'ı Linux'tan yavaş ve pahalıdır;
  LocalDB örneği kendiliğinden bir süre açık kalır — test adımı örneği kendi içinde durdurmalıdır
  (`docs/testing.md`, Resource hygiene); LocalDB sürümü üretim SQL Server sürümünden farklı olabilir.

## Alternatives considered
- Testcontainers (SQL Server container): platformdan bağımsız ve izole, ama Docker engine
  gerektirir; insan kararıyla seçilmedi.
- EF InMemory: gerçek SQL davranışını test etmez.

## Revisit triggers
- GitHub Windows runner'ında LocalDB yoksa veya kurulumu kırılgansa.
- LocalDB ile üretim SQL Server sürümü arasında davranış farkı bir hataya yol açarsa.
- Geliştirme ortamında Docker standart hale gelirse.
