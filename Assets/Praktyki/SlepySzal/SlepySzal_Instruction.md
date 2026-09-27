# 👁️‍🗨️ Poradnik Implementacji: Eliksir "Ślepy Szał" (SlepySzal)

Kompletny system dla eliksiru **"Ślepy Szał"** (+200% obrażeń, 3x szybszy pasywny ubytek zdrowia, drastycznie ograniczona widoczność czarną winietą tunelową do 15% środka ekranu, efekt trwały do momentu wyleczenia) został w pełni wdrożony i zintegrowany z projektem.

Folder główny: `Assets/Praktyki/SlepySzal/`

---

## 📂 Zawartość folderu `Assets/Praktyki/SlepySzal/`

| Plik | Opis |
| :--- | :--- |
| **[`SlepySzalBuff.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SlepySzal/SlepySzalBuff.cs)** | Komponent gracza zarządzający stanem Ślepego Szału: mnożnik ataku x3.0 (+200%), mnożnik ubytku HP x3.0. Działa permanentnie aż do wywołania metody `SlepySzalBuff.CureBuff()` lub śmierci postaci. |
| **[`SlepySzalItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SlepySzal/SlepySzalItem.cs)** | Klasa przedmiotu (ScriptableObject dziedziczący po `EquippableItem`), obsługująca trzymanie w dłoni (`heldSprite`), animację picia (`useItemRotation: 1`) oraz nałożenie trwałego szału. |
| **[`SlepySzalPotion.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SlepySzal/SlepySzalPotion.cs)** | Skrypt na obiekt w świecie gry (World Pickup ze skalą `0.08`, kolizją trigger / interakcją, automatycznym `Billboardem` do kamery, **bez magnesu**). Podnosi butelkę bezpośrednio do ekwipunku gracza (EQ). |
| **[`SlepySzalUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SlepySzal/SlepySzalUI.cs)** | Proceduralna, ekstremalnie głęboka czarna winieta tunelowa (*Tunnel Vision*) z czystym środkiem o promieniu zaledwie `0.06` (otoczona 100% nieprzeniknioną czernią). |
| **`SlepySzal.png`** | Grafika butelki Ślepego Szału. |
| **`SlepySzal.asset`** | Gotowy i skonfigurowany plik ScriptableObject przedmiotu z parametrami rotacji i opisem. |
| **`SlepySzal.prefab`** | Gotowy prefab do postawienia w świecie gry jako znajdźka (skala `0.08`). |
| **`Assets/Praktyki/Shop/SlepySzal_ShopItem.asset`** | Towar do sklepu handlarza NPC (cena: 30 kapsli). |

---

## ⚙️ Wprowadzone Integracje w Projekcie

1. **Obrażenia Gracza ([`PlayerStats.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerStats.cs))**:
   - W metodzie `GetDamage(int baseDamage)` wprowadzono mnożnik `SlepySzalBuff.DamageMultiplier` (3.0x, czyli bazowe obrażenia + 200%), gdy szał jest aktywny.
2. **Pasywny Ubytek Zdrowia ([`PlayerHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHealth.cs))**:
   - W pętli `Update()` szybkość ubytku zdrowia (`healthDecayAccumulator`) zostaje pomnożona przez `SlepySzalBuff.DecayMultiplier` (3x szybciej).
   - W metodzie `Die()` efekt szału zostaje automatycznie zresetowany po zgonie gracza.
3. **Ręczna Regulacja Pola Widzenia w Inspektorze**:
   - Możesz w każdej chwili zmienić pole widzenia bez modyfikacji kodu:
     - W pliku [`SlepySzal.asset`](file:///g:/beergeon_v2/Assets/Praktyki/SlepySzal/SlepySzal.asset) dostępne są suwaki:
       - **Vision Clear Radius**: wielkość czystego otworu w centrum (np. `0.05` = skrajnie wąski tunel, `0.10` = domyślny, `0.20` = szeroki).
       - **Vision Outer Radius**: odległość, przy której czerń staje się w 100% nieprzenikniona.
     - **Testowanie na żywo w trakcie gry**: Klikając na obiekt `SlepySzal_UI` pod Canvasem w trakcie gry (Play Mode), przesuwanie suwaków natychmiastowo w czasie rzeczywistym aktualizuje widok na ekranie!
4. **Funkcja Leczenia Pod Następnego Potiona**:
   - Skrypt [`SlepySzalBuff.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SlepySzal/SlepySzalBuff.cs) posiada gotową publiczną metodę:
     ```csharp
     SlepySzalBuff.CureBuff();
     ```
     Wystarczy, że przyszła potka lecznicza / antidotum wywoła tę jedną linijkę, aby natychmiast zdjąć szał, przywrócić normalny wzrok i standardowe tempo ubytku HP.
4. **Sklep NPC (`PauseMenu.prefab`)**:
   - Towar dodany do puli sklepu u handlarza z ceną **30 kapsli**.
5. **Drop z Przeciwników ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs) & `Hmielnik.prefab`)**:
   - Skonfigurowano rzadki drop o szansie **3%** (`0.03f`) po pokonaniu potwora.

---

## 🚀 Jak Przetestować w Grze

1. **Uruchom grę w Unity Editorze**.
2. **Wypicie mikstury**:
   - Wybierz Ślepy Szał w ekwipunku i kliknij **LPM (Lewy Przycisk Myszy)**:
     - Na ekranie natychmiast pojawi się głęboki, czarny tunel widzenia zostawiający zaledwie mały otwór na środku.
     - Zdrowie zacznie ubywać 3-krotnie szybciej!
     - Twoje ciosy będą zadawać **3x większe obrażenia (+200%)**!
     - W konsoli pojawi się komunikat:
       `[Ślepy Szał] Aktywowano Ślepy Szał! Obrażenia x3, ubytek zdrowia x3 szybszy. Efekt trwa do momentu wyleczenia!`
3. **Test Zdejmowania Szału**:
   - Po śmierci gracza wzrok i zdrowie wracają do normy, a po stworzeniu antidotum wywołanie `SlepySzalBuff.CureBuff()` zdejmie efekt od razu.
