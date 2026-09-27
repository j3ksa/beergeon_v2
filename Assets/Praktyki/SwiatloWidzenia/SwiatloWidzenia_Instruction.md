# 💧 Poradnik Implementacji: Mikstura "Światło Widzenia" (SwiatloWidzenia)

Kompletny system dla mikstury leczniczej **"Światło Widzenia"** (antidotum zdejmującego stan Ślepego Szału, przywracającego pełny wzrok i normalne tempo ubytku zdrowia, z ceną 40 kapsli i 2% szansą na drop) został w pełni zaimplementowany i połączony z projektem.

Folder główny: `Assets/Praktyki/SwiatloWidzenia/`

---

## 📂 Zawartość folderu `Assets/Praktyki/SwiatloWidzenia/`

| Plik | Opis |
| :--- | :--- |
| **[`SwiatloWidzeniaItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SwiatloWidzenia/SwiatloWidzeniaItem.cs)** | Klasa przedmiotu ekwipunku (`EquippableItem`). Po wypiciu wywołuje `SlepySzalBuff.CureBuff()`, natychmiastowo zdejmując ciemną winietę, przywracając bazowy poziom obrażeń oraz normalne tempo życia, a także lecząc 15 HP. |
| **[`SwiatloWidzeniaPotion.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SwiatloWidzenia/SwiatloWidzeniaPotion.cs)** | Skrypt na obiekt w świecie gry (World Pickup ze skalą `0.08`, `Billboardem`, **bez magnesu**). Zgodnie z wytycznymi podnosi się wyłącznie klawiszem **`E`** (IInteractable) do ekwipunku gracza (wejście ciałem nic nie robi). |
| **`SwiatloWidzenia.png`** | Niebieska butelka z etykietą i zawieszką. |
| **`SwiatloWidzenia.asset`** | Skonfigurowany ScriptableObject przedmiotu z animacją bujania w dłoni (`useItemRotation: 1`). |
| **`SwiatloWidzenia.prefab`** | Gotowy prefab do postawienia w świecie gry jako znajdźka (skala `0.08`). |
| **`Assets/Praktyki/Shop/SwiatloWidzenia_ShopItem.asset`** | Towar do sklepu handlarza NPC z ceną **40 kapsli** (10 kapsli więcej niż Ślepy Szał). |

---

## ⚙️ Wprowadzone Integracje w Projekcie

1. **Uleczenie Ślepego Szału**:
   - Wypicie Światła Widzenia natychmiast wywołuje `Praktyki.SlepySzal.SlepySzalBuff.CureBuff()`:
     - Wyłącza czarną winietę na ekranie i przywraca 100% normalnego wzroku.
     - Przywraca standardowy mnożnik obrażeń (usuwa buff +200%).
     - Przywraca bazowe, normalne tempo pasywnego spadku HP (usuwa 3x decay).
2. **Sklep NPC (`PauseMenu.prefab`)**:
   - Towar dodany do puli sklepu z ceną **40 kapsli**.
3. **Drop z Przeciwników ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs) & `Hmielnik.prefab`)**:
   - Skonfigurowano drop z szansą **2%** (`0.02f`) po pokonaniu wroga.
4. **Sterowanie i Ekwipunek**:
   - Podnoszenie butelki ze świata gry odbywa się **wyłącznie klawiszem `E`** do ekwipunku gracza, z widokiem butelki w dłoni i animacją przechylenia przy piciu.
