# 🛡️ Poradnik Implementacji: Mikstura "Ognista Woda" (Firewater)

Kompletny system dla nowego potiona **"Ognista Woda"** (mikstury redukującej otrzymywane obrażenia o 25% na 10 sekund z subtelną, delikatną szarą winietą UI) został w pełni przygotowany i zintegrowany w projekcie.

Folder główny: `Assets/Praktyki/OgnistaWoda/`

---

## 📂 Zawartość folderu `Assets/Praktyki/OgnistaWoda/`

| Plik | Opis |
| :--- | :--- |
| **[`OgnistaWodaBuff.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OgnistaWoda/OgnistaWodaBuff.cs)** | Komponent gracza zarządzający stanem buffa, czasem trwania (10s) oraz procentem redukcji obrażeń (25%). |
| **[`OgnistaWodaItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OgnistaWoda/OgnistaWodaItem.cs)** | Klasa przedmiotu (ScriptableObject dziedziczący po `EquippableItem`), obsługująca trzymanie butelki w dłoni (`heldSprite`), animację przechylenia przy piciu i nałożenie buffa. |
| **[`OgnistaWodaPotion.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OgnistaWoda/OgnistaWodaPotion.cs)** | Skrypt na obiekt w świecie gry (World Pickup z kolizją trigger, automatycznym obracaniem `Billboard` do kamery, **bez magnesu** – dokładnie tak jak Bycze Paliwo i Mocny Gul). |
| **[`OgnistaWodaUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/OgnistaWoda/OgnistaWodaUI.cs)** | Skrypt efektu wizualnego UI – proceduralna, bardzo delikatna, miękka szara winieta wokół ekranu, widoczna przez czas trwania buffa (10s). |
| **`OgnistaWoda.png`** | Grafika butelki Ognistej Wody. |
| **`OgnistaWoda.asset`** | Gotowy i skonfigurowany plik ScriptableObject przedmiotu z przypisaną ikoną i parametrami. |
| **`OgnistaWoda.prefab`** | Gotowy prefab do postawienia w świecie gry jako znajdźka. |
| **`Assets/Praktyki/Shop/OgnistaWoda_ShopItem.asset`** | Gotowy plik towaru do sklepu u handlarza NPC (cena: 10 kapsli). |

---

## ⚙️ Wprowadzone Integracje w Projekcie

1. **Gracz ([`PlayerHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHealth.cs))**:
   - W metodzie `TakeDamage(int damage)` dodano sprawdzanie `OgnistaWodaBuff.IsActive`.
   - Gdy buff jest aktywny, każde otrzymane obrażenia są pomniejszane o 25% (z zaokrągleniem do min. 1 dmg).
2. **Przeciwnicy ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs) & `Hmielnik.prefab`)**:
   - Dodano pola `ognistaWodaPrefab` i `ognistaWodaDropChance` (domyślnie 20% szansy na drop po zabiciu wroga).
3. **Sklep ([`ShopManagerUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Shop/ShopManagerUI.cs) w `PauseMenu.prefab`)**:
   - Dodano `OgnistaWoda_ShopItem` do puli przedmiotów sklepu handlarza (`Item Pool`).
4. **Trzymanie w dłoni ([`PlayerHand.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHand.cs))**:
   - Po wybraniu Ognistej Wody w aktywnym slocie ekwipunku, butelka pojawia się w dłoni gracza, a kliknięcie LPM uruchamia animację picia i aktywuje buff.

---

## 🚀 Jak Przetestować w Grze

1. **Uruchom grę w Unity Editorze**.
2. **Zakup lub zdobycie**:
   - Kup Ognistą Wodę w sklepie u NPC lub podnieś z mapy / po pokonaniu przeciwnika.
3. **Picie**:
   - Wybierz Ognistą Wodę na pasku slotów (pojawi się w dłoni gracza).
   - Kliknij **LPM (Lewy Przycisk Myszy)**:
     - Wokół ekranu pojawi się lekka, miękka szara winieta na 10 sekund.
     - W konsoli pojawi się log: `[Ognista Woda] Aktywowano ochronę! Otrzymywane obrażenia mniejsze o 25% przez 10.0s.`
4. **Test Redukcji Obrażeń**:
   - Daj się uderzyć przeciwnikowi podczas trwania efektu – w konsoli zobaczysz informację o zredukowaniu obrażeń (np. z 10 do 8, z 20 do 15 itp.).
