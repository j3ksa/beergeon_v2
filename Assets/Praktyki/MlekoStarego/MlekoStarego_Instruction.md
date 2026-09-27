# Dokumentacja Praktyk: Mikstura "Mleko Starego" (Antidotum na Truciznę)

## 1. Cel i Założenia Projektowe
Celem zadania było zaprojektowanie, oprogramowanie i wdrożenie do gry nowej mikstury użytkowej **"Mleko Starego"**, pełniącej rolę strategicznego antidotum w walce z przeciwnikiem **Stary Chmielnik**.

### Główne wymagania:
1. **Niwelowanie trucizny**: Natychmiastowe zdjęcie aktywnego efektu zatrucia (`HopPoisonDebuff`) zadanego przez szyszki Starego Chmielnika (zatrzymanie obrażeń 10 HP/s i przywrócenie czerwonego paska zdrowia).
2. **Czasowa odporność (10s)**: Zagwarantowanie graczowi 10 sekund całkowitej odporności na truciznę (`MlekoStaregoBuff.IsImmune`), uniemożliwiając nałożenie ponownego zatrucia.
3. **Drop z przeciwników (10%)**: Każdy pokonany wróg posiada 10% szansy na upuszczenie butelki Mleka Starego na ziemię.
4. **Obecność w sklepie (15 kapsli)**: Dostępność do zakupu u handlarza za 15 kapsli w ramach systemu `ShopSystem`.
5. **Wygodne podnoszenie**: Możliwość podniesienia butelki do ekwipunku klawiszem `[E]` lub poprzez wejście w nią (z zabezpieczeniem wypicia z ziemi, gdy ekwipunek jest pełny).

---

## 2. Architektura Systemu

### Utworzone i zmodyfikowane pliki:
1. `Assets/Praktyki/MlekoStarego/MlekoStaregoBuff.cs`
   - Zarządza stanem odporności (`IsImmune`), czasem trwania (10s) oraz procedurą oczyszczenia (`ActivateImmunity`).
   - Zdejmuje `HopPoisonDebuff` i opcjonalnie `HopSlowDebuff`.
2. `Assets/Praktyki/MlekoStarego/MlekoStaregoItem.cs`
   - Rozszerzenie klasy `EquippableItem`. Obsługuje użycie z poziomu ekwipunku (`Use`), odtwarzanie efektów i usunięcie zużytej butelki.
3. `Assets/Praktyki/MlekoStarego/MlekoStaregoPotion.cs`
   - Skrypt obiektu w świecie gry (World Pickup). Implementuje `IInteractable` (klawisz E), auto-orientację do kamery (`Billboard`) oraz detekcję wejścia w trigger.
4. `Assets/Praktyki/MlekoStarego/MlekoStaregoUI.cs`
   - Proceduralny efekt wizualny: biały rozbłysk oczyszczenia w momencie wypicia oraz delikatna, pulsująca mleczna aura na krawędziach ekranu przez 10s trwania tarczy.
5. `Assets/Praktyki/StaryHmielnik/HopPoisonDebuff.cs` i `HopConeProjectile.cs`
   - Zintegrowano warunek `!MlekoStaregoBuff.IsImmune`, blokujący możliwość zatrucia gracza pod wpływem mleka.
6. `Assets/Scripts/Enemy/EnemyHealth.cs`
   - Dodano szansę na drop (10%) mikstury `MlekoStaregoPickup` po śmierci dowolnego przeciwnika.
7. `Assets/Resources/ShopItems/MlekoStarego_ShopItem.asset`
   - Asset konfiguracyjny przedmiotu w sklepie (cena: 15 kapsli).
8. `Assets/Resources/Items/MlekoStaregoPickup.prefab` & `Assets/Praktyki/MlekoStarego/MlekoStarego.asset`
   - Prefab fizycznego pickupa w świecie 3D oraz ScriptableObject danych przedmiotu do ekwipunku.

---

## 3. Etapy Realizacji i Szacowany Czas Pracy (Do Dziennika Praktyk)

| Etap | Opis prac | Szacowany czas |
| :--- | :--- | :---: |
| **Etap 1: Analiza i Projekt Architektury** | Analiza interakcji z debuffami Starego Chmielnika, określenie mechaniki oczyszczania i odporności, rozplanowanie struktury plików w `Assets/Praktyki/MlekoStarego/`. | **45 min** |
| **Etap 2: Logika Buffa i Odporności (C#)** | Oprogramowanie `MlekoStaregoBuff.cs`: zdjęcie coroutine zatrucia, przywrócenie koloru UI życia, 10-sekundowy timer ochronny. Integracja z `HopPoisonDebuff` i pociskami szyszek. | **1 godz.** |
| **Etap 3: Integracja z Ekwipunkiem i Przedmiotem** | Utworzenie `MlekoStaregoItem.cs` (dziedziczenie po `EquippableItem`), obsługa konsumpcji, assetu ScriptableObject i parametrów animacji/trzymania w ręku. | **45 min** |
| **Etap 4: Efekty Wizualne i UI (Mleczna Aura)** | Oprogramowanie `MlekoStaregoUI.cs`: proceduralna tekstura winiety ekranowej, biały rozbłysk oczyszczenia oraz delikatna mleczna ramka ochronna. | **45 min** |
| **Etap 5: Fizyczny Pickup w Świecie Gry** | Konfiguracja `MlekoStaregoPotion.cs`, prefabu ze SpriteRendererem, Billboardem (zawsze przodem do gracza), BoxColliderem (trigger) oraz obsługą klawisza `E`. | **45 min** |
| **Etap 6: Integracja z Dropem z Wrogów i Sklepem** | Dodanie 10% szansy na drop w `EnemyHealth.cs`, konfiguracja `ShopItemSO` w sklepie za 15 kapsli, testy i dokumentacja. | **45 min** |
| **Łączny czas realizacji:** | | **~5 godz.** |
