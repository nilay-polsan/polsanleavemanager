# ADR 0003 — Mimari testler için ArchUnitNET

- Status: Accepted
- Date: 2026-10-21

## Context
`docs/architecture.md` Forbidden dependencies'i NetArchTest ile zorlamayı öngörüyordu. Walking
skeleton BUILD'inde `NetArchTest.Rules`'ın son sürümünün 2021-05-23 tarihli olduğu görüldü;
`docs/security.md` "son 12 ayda bakım" şartı sağlanmıyor.

## Decision
Mimari sınır testleri `TngTech.ArchUnitNET` (çekirdek paket, xUnit adaptörü olmadan) ile yazılır;
NetArchTest kullanılmaz.

## Consequences
- Kazanç: Apache-2.0; 0.13.4 sürümü 2026-08-20 tarihli (aktif bakım); bağımlılıkları (Mono.Cecil,
  Newtonsoft.Json, CycleDetection, JetBrains.Annotations) izin verilen lisanslarda; assembly,
  namespace ve tip bağımlılığı kuralları Forbidden dependencies 1–4'ü karşılar.
- Bedel: sürüm 1.0 öncesi (API değişebilir); NetArchTest'ten farklı API — kurallar bir kez yazılır.

## Alternatives considered
- NetArchTest.Rules: bakım şartını karşılamıyor (son sürüm 2021).
- NetArchTest istisnası (ADR ile kabul): güvenlik kuralını gevşetirdi; aktif alternatif varken gereksiz.
- Elle yansıma (reflection) testleri: kural başına tekrar eden kod, daha kırılgan.

## Revisit triggers
- ArchUnitNET 12 aydan uzun süre yayın yapmazsa veya kural ihtiyacımızı karşılamazsa.
- Bilinen bir CVE çıkarsa.
