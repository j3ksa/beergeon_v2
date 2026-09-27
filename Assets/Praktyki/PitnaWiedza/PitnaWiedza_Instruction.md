# ✨ Poradnik Implementacji: Mikstura "Pitna Wiedza" (+1 Poziom)

Kompletny system dla nowego eliksiru **"Pitna Wiedza"** (mikstury natychmiastowo podnoszącej poziom postaci o +1 lvl, ze złotą poświatą UI oraz dynamicznym skalowaniem ceny i rzadkości dropu z poziomem gracza) został w pełni zaimplementowany i zintegrowany w projekcie.

Folder główny: `Assets/Praktyki/PitnaWiedza/`

---

## 📂 Zawartość folderu `Assets/Praktyki/PitnaWiedza/`

| Plik | Opis |
| :--- | :--- |
| **[`PitnaWiedzaItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/PitnaWiedza/PitnaWiedzaItem.cs)** | Klasa przedmiotu (ScriptableObject dziedziczący po `EquippableItem`), obsługująca trzymanie butelki w dłoni (`heldSprite`), animację bujania przy piciu (`useItemRotation: 1`) oraz natychmiastowy awans o +1 poziom. |
| **[`PitnaWiedzaPotion.cs`](file:///g:/beergeon_v2/Assets/Praktyki/PitnaWiedza/PitnaWiedzaPotion.cs)** | Skrypt na obiekt w świecie gry (World Pickup ze skalą `0.08`, kolizją trigger / interakcją, automatycznym obracaniem `Billboard` do kamery, **bez magnesu**). Podnosi miksturę bezpośrednio do ekwipunku gracza (EQ). |
| **[`PitnaWiedzaUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/PitnaWiedza/PitnaWiedzaUI.cs)** | Skrypt efektu wizualnego UI – proceduralna, promieniująca złota poświata i rozbłysk winiety wokół ekranu w momencie awansu poziomu. |
| **`PitnaWiedza.png`** | Grafika butelki Pitnej Wiedzy. |
| **`PitnaWiedza.asset`** | Gotowy i skonfigurowany plik ScriptableObject przedmiotu z przypisaną ikoną, opisem i parametrami bujania. |
| **`PitnaWiedza.prefab`** | Gotowy prefab do postawienia w świecie gry jako znajdźka (skala `0.18`). |
| **`Assets/Praktyki/Shop/PitnaWiedza_ShopItem.asset`** | Gotowy plik towaru do sklepu u handlarza NPC (cena bazowa: 10 kapsli). |

---

## ⚙️ Wprowadzone Integracje w Projekcie

1. **System Poziomów ([`PlayerStats.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerStats.cs))**:
   - Dodano metodę `LevelUpDirectly(int levelsToAdd = 1)`, która bezpośrednio podnosi poziom gracza (`level++`), zachowując dotychczasowy postęp punktów doświadczenia do kolejnego poziomu, oraz wywołuje zdarzenia `OnLevelUp` i `OnStatsChanged`.
2. **Dynamiczne Skalowanie Cen w Sklepie ([`ShopManagerUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Shop/ShopManagerUI.cs) w `PauseMenu.prefab`)**:
   - Towar `PitnaWiedza_ShopItem` startuje od ceny bazowej **10 kapsli** na 1 poziomie gracza i automatycznie drożeje wraz ze zdobywaniem kolejnych poziomów (`1f + (playerLevel - 1) * priceMultiplierPerLevel`).
3. **Dynamiczne Skalowanie Dropu z Przeciwników ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs) & `Hmielnik.prefab`)**:
   - Bazowa szansa na drop wynosi **13%** na poziomie 1 i stopniowo maleje na wyższych levelach postaci, zachowując idealny balans ekonomii i progresji w grze.
4. **Trzymanie w dłoni ([`PlayerHand.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHand.cs))**:
   - Po wybraniu Pitnej Wiedzy w slocie ekwipunku butelka pojawia się w ręku postaci, a kliknięcie LPM uruchamia animację przechylenia butelki i natychmiast przyznaje awans.

---

## 🚀 Jak Przetestować w Grze

1. **Uruchom grę w Unity Editorze**.
2. **Zdobycie mikstury**:
   - Kup Pitną Wiedzę w sklepie u NPC (lub pokonaj wrogów / umieść `PitnaWiedza.prefab` na scenie).
3. **Wypicie**:
   - Wybierz miksturę na pasku slotów i kliknij **LPM (Lewy Przycisk Myszy)**:
     - Poziom postaci natychmiast wzrośnie o **+1**!
     - Na ekranie pojawi się złoty rozbłysk winiety.
     - Pasek XP zaktualizuje się automatycznie.
     - W konsoli pojawi się komunikat:
       `[Pitna Wiedza] Gracz wypił Pitną Wiedzę! Poziom wzrósł o +1 (Aktualny poziom: X)!`
