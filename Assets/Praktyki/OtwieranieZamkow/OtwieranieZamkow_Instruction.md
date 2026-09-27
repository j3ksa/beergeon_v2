# System Kłódki, Wytrychów i Otwierania Zamków (Minigra Sweet Spot)

System zablokowanych drzwi, interaktywnej kłódki na drzwiach oraz minigry zręcznościowej w stylu **Sweet Spot / Timing Bar** (jak w Dead by Daylight / Gothic) w uniwersum Beergeon.

---

## 📦 Zawartość i Pliki Systemu

Wszystkie skrypty i zasoby znajdują się w: `Assets/Praktyki/OtwieranieZamkow/`:

1. **[`LockpickItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OtwieranieZamkow/LockpickItem.cs)**
   - Klasa przedmiotu ScriptableObject (`Item`).
   - Wytrychy stackują się do **64 sztuk** w jednym slocie (`isStackable = true`, `maxStack = 64`).

2. **[`PadlockController.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OtwieranieZamkow/PadlockController.cs)**
   - Komponent na obiekt kłódki (`klodka`).
   - Blokuje przypisane lub najbliższe drzwi (`DoorController.SetLocked(true)`).
   - Implementuje `IInteractable` – wyświetla podpowiedź celownika:
     - Jeśli gracz ma wytrychy: `Otwórz kłódkę (Wytrychy: X) [E]`
     - Jeśli gracz nie ma wytrychów: `Kłódka - Wymaga wytrychu!`
   - Po udanym otwarciu:
     - Drzwi zostają trwale odblokowane (`isLocked = false`).
     - Kłódka odpina się z uchwytu, dostaje fizykę (`Rigidbody`) i z brzękiem spada na podłogę.

3. **[`LockpickingMinigameUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OtwieranieZamkow/LockpickingMinigameUI.cs)**
   - Minigra zręcznościowa na osobnym, eleganckim Canvasie.
   - Płynnie poruszający się wskaźnik/iglica.
   - Dynamiczna zielona strefa sukcesu (**Sweet Spot**) oraz wąskie złote pole (**Crit Spot**).
   - Wymaga zaliczenia **3 zapadek** (1/3 -> 2/3 -> 3/3).
   - Sterowanie:
     - **`[SPACJA]`**, **`[E]`** lub **`[LPM]`** – próba podważenia zapadki.
     - **`[ESC]`** – przerwanie otwierania bez straty wytrycha.
   - Pomyłka: wstrząs wskaźnika, czerwony błysk i **złamanie 1 wytrycha**. Jeśli skończą się wytrychy – minigra kończy się porażką.
   - Posiada wbudowane proceduralne efekty dźwiękowe (kliknięcie zapadki, pęknięcie wytrycha, szczęk otwarcia).

4. **[`LockpickPickup.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OtwieranieZamkow/LockpickPickup.cs) / `WytrychPickup.prefab`**
   - Prefab wytrychu w świecie gry ze sprite'em, billboardem i animacją lewitacji.

5. **Narzędzia Unity Editor ([`LockpickingSetupEditor.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OtwieranieZamkow/Editor/LockpickingSetupEditor.cs))**
   - Menu w Unity: **`Tools -> Otwieranie Zamków`**:
     - `1. Skonfiguruj Kłódkę`: automatycznie dodaje collider, `PadlockController` i blokuje najbliższe drzwi.
     - `2. Dodaj Wytrych (Pickup) obok gracza`: tworzy w świecie gry wytrych do natychmiastowego podniesienia i testu.

---

## 🛒 Sklep i Drop z Przeciwników

* **Sklep u Handlarza (`MerchantNPC`):**
  - Przedmiot: `Wytrych_ShopItem.asset`
  - Cena: **15 kapsli** za pakiet **3 sztuk** wytrychów.
  - Automatycznie pojawia się w puli towarów w sklepie (`Resources/ShopItems/`).

* **Drop z pokonanych wrogów (`EnemyHealth.cs`):**
  - Szansa: **8%** na upuszczenie wytrychu przez przeciwnika.
