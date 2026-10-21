# Testing

> Bootstrap'ta karara bağlandı (2026-10-19).

## The contract
- Every acceptance criterion maps to at least one test (criterion ↔ test map lives in the plan).
- Tests assert **behavior**, not implementation details or mere status codes.
- The whole suite runs inside `scripts/check` — one command, everywhere.

## Frameworks & layout
- **xUnit v3** — birim ve entegrasyon testleri. **bUnit** — Blazor bileşen testleri.
- **NetArchTest** — mimari sınır testleri (`tests/PolsanLeaveManager.ArchitectureTests`).
- **SQL Server LocalDB** — entegrasyon testleri gerçek SQL Server motoruna karşı (EF InMemory
  yasak). Testler adanmış `PolsanLeaveTests` örneğini kullanır; test adımı bitince örneği
  durdurur (Resource hygiene). Önkoşul: Windows + LocalDB (ADR-0002). CI runner'ı Windows'tur.
- **Playwright for .NET** — e2e, 3–5 kritik akış (talep aç → onayla → bakiye düşer; ret;
  avans). Headless, tek worker. Uygulamayı (Kestrel, test ortamı) test fixture'ı başlatır ve
  başarısızlık/timeout dahil her durumda durdurur — app-under-test yaşam döngüsünün tek sahibi fixture'dır.
- Yerleşim: `tests/PolsanLeaveManager.<Module>.Tests`, `tests/PolsanLeaveManager.Web.Tests`
  (bUnit), `tests/PolsanLeaveManager.E2E`.
- Hız ayrımı **trait ile** (selector, liste değil): `[Trait("Category","Integration")]`,
  `[Trait("Category","E2E")]`. Etiketsiz test = hızlı test. Fast profil etiketsizleri, full-only
  adımlar yalnızca etiketlileri koşar → "fast ∪ geri kalan = tüm testler".
- Test isimleri: `docs/conventions.md`, Naming.

## What must be tested
- Her iş kuralı (BR-n, `docs/domain.md`) en az bir testle; test adında kural no geçer.
- Her yasak bağımlılık (`docs/architecture.md`) bir mimari testle.
- Her yetki kuralı (`docs/security.md`): rol başına izinli **ve** reddedilen yol, ayrıca başkasının
  kaydına erişim denemesi.
- İş günü/bakiye hesabı sınır değerleri: yıldönümü günü, kıdem 5/15 yıl geçişleri, 18/50 yaş,
  arife, yıl dönümü, hafta sonuna denk gelen tatil.
- Ledger değişmezliği: bakiye = hareket toplamı; iptal ters kayıt üretir.
- Kritik akışlar e2e (yukarıda).
- Yeniden hesap (BR-24, BR-25): bekleyen talep güncellenir; onaylı talepte İK onayına kadar
  ledger değişmez; aynı takvim sürümü için ikinci çalıştırma ikinci düzeltme üretmez (idempotent).
- İptal (BR-26…BR-29): rol × durum × zaman matrisi; "başlamış" sınırı `TimeProvider` ile
  başlangıç günü 23:59:59 / 00:00:00 (Europe/Istanbul) sınır değerleriyle.
- FIFO tahsis (BR-30, BR-31): tahsis toplamı = kullanım; en eski yıl önce tükenir; iptal ve
  kısaltma tahsisi geri alır.
- Bölme kuralı (BR-21): her kontrol × `Off` / `Warn` / `Block`; zorunlu kontrolde `Off` reddedilir.
- Son HR koruması (BR-32): iki eşzamanlı rol kaldırma işlemi (gerçek SQL Server, Integration)
  sonunda en az bir aktif HR kalır. Rol iptali açık bir Blazor oturumunda bir sonraki çağrıda
  yetkisiz döner (BR-34).

## Protected-tests rule
Weakening asserts, deleting, or skipping tests to reach green is forbidden. A red test triggers
`prompts/recovery/red-test.md` (R-02) — first decide what is wrong: code, test, or spec.

## Characterization tests
Before refactoring untested code (`workflows/refactor.md`) and for brownfield change requests
(`workflows/change-request.md`, Preserved behavior), pin the current behavior first — warts
included. They are written from observation, not from what the code "should" do.

## Evidence for UI criteria
A screenshot is evidence for a UI criterion; for change requests, before/after screenshots that
also show the preserved behavior. Screenshots go into the PR, not the repo (Evidence policy).
A screenshot proves a criterion only against the spec's "User experience": every state it lists
(empty, loading, error, success, no permission), compared with the linked design if there is one.

## Evidence policy
1. **Evidence is referenced, not stored.** Each acceptance criterion's evidence is a pointer: the
   test's name and the command that reproduces it (`./scripts/check`). The criterion ↔ evidence
   table in the spec/plan is the only evidence artifact that lives in the repo.
2. **Reproducible evidence is never filed.** No saved check output, console dumps, coverage reports
   or log copies in the repo — git history and CI runs already archive every execution.
3. **Non-reproducible evidence** (a screenshot of a manual UI check, a one-off measurement, an
   external-system confirmation) goes into the pull request (description or attachment); the
   evidence table links to it. It is the exception, not the rule.
4. **Never clone or copy the repository inside its own working tree.** Clean-clone verification
   runs in a temporary directory outside the repo; only the one-line result (command + green/red)
   is recorded in the evidence table. The clone is always deleted, never kept.
5. **There is no `evidence/` (or `.evidence/`) directory in this workspace.** An agent that feels
   the need to create one is about to violate rule 1. `scripts/doctor` warns on both violations.

## Resource hygiene
1. **A check run leaves nothing running.** No survivors of any kind: build/compile daemons,
   package-manager services, test hosts and parallel workers, watch modes, browsers, the application
   under test, containers or emulators started for the run. Agents run check an order of magnitude
   more often than humans; per-run residue compounds into machine starvation.
2. **The residue test** (the rule's definition, not advice): run `./scripts/check` twice back to
   back; after each run, process count and memory return to the baseline measured before the first.
   `./scripts/residue` runs it (`--fast` for the fast profile) and names every survivor with its
   command line; it lists anything started meanwhile too, so run it on an otherwise idle machine.
   Rerun it whenever `scripts/check.conf` changes.
3. Warm-process caches trade memory for speed for humans who build rarely; with an agent, trade the
   seconds back for a flat memory profile.
4. Interactive modes (watchers, dev servers, REPLs) never belong in check. e2e runs single-worker
   and headless by default; one component owns the app-under-test lifecycle — started for the run,
   stopped at the end, including on failure and timeout.

## Two speeds, one contract
1. `./scripts/check` is still the one way to ask "is this good?"; `--fast` is a speed of the
   contract, not a second contract.
2. **Fast green is a working state; only full green is evidence.** Per step, per finding, per
   iteration → `--fast`. Segment exits, PROVE steps, recovery-ramp exits, the verify table and CI
   → `CHECK GREEN (full, …)`. `scripts/doctor` warns when a shipped spec, or in CI the pull
   request description, shows `CHECK GREEN (fast, …)` and no `(full, …)`.
3. Fast: compile and the quick tests (unit, lint). Full only: e2e, long integration suites, and
   any single step over ~a minute — the minute is a per-step threshold, not a total for the fast
   profile; fast has no fixed total. Bootstrap decides per stack and tags the steps `[fast]` in
   check.conf. When fast grows too long to iterate with, take steps out in this order: lint
   first, then formatting, then unit tests; compilation stays longest.
4. **Split a test step by a selector, never by a list.** The fast step selects with a filter
   (a category, trait, tag or namespace) and the full-only step runs the same filter inverted,
   so "fast ∪ the rest = every test" holds by construction: a test added later still runs in
   one of them. Splitting by naming projects or files leaves a silent hole — a new one that is
   not added to check.conf runs in neither profile while check stays green.
5. Durations come from check itself (`<== <step> (n s)` per step, the total in the green line).
   Decide a split, or report a profile's duration, on the median of at least 3 runs — single
   runs vary too much to decide on.

## Open technical findings
- **TF-1 — Bootstrap doğrulaması kabul edilmedi (2026-10-19).** `scripts/check.conf` adımları
  yorumda olduğundan `./scripts/check` gerçek bir build/test çalıştırmıyor (greenfield no-op);
  yeşil sonuç kanıt değildir. Kapanış koşulu: walking skeleton spec'inde adımlar aktif,
  `CHECK GREEN (full, …)` ve her test adımının koşturduğu test sayısı > 0 kanıtlı.
- **TF-2 — Fast residue testi başarısız (2026-10-19).** `./scripts/residue --fast` iki denemede
  `RESIDUE FOUND` verdi; kalanlar: `svchost.exe`, `backgroundTaskHost.exe` (Spotlight),
  `RuntimeBroker.exe`. Check hiç adım çalıştırmadığından bunlar büyük olasılıkla Windows arka plan
  aktivitesidir, ama doğrulanmadı. Full residue aynı makinede `RESIDUE CLEAN`. Kapanış koşulu:
  adımlar aktifken boşta bir makinede `./scripts/residue` ve `./scripts/residue --fast` ikisi de
  `RESIDUE CLEAN`; değilse her kalan süreç bir check adımına bağlanır veya bağlanamayacağı gösterilir.

## Determinism
Flaky tests are fixed, not retried or skipped — see R-03. Evidence of a fix: 5 consecutive green runs.
