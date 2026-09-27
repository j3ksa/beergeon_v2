# 🥩🔨 Poradnik Implementacji: Młotek Do Kotletów (Meat Tenderizer)

Kompletny system dla nowej broni obuchowej **Młotek Do Kotletów** (ciężki kuchenny tłuczek do mięsa zadający 45 obrażeń przy powolnym ataku podstawowym z ubytkiem staminy i 10% szansą na odrzut/ogłuszenie, oraz posiadający potężny alternatywny atak PPM zużywający całą staminę, zadający 60 obrażeń, mocno odrzucający i ogłuszający wrogów kosztem chwilowego spowolnienia gracza, z dropem 2% z wrogów i ceną 70 kapsli w sklepie) został w pełni zaimplementowany i zintegrowany z projektem.

Folder główny: `Assets/Praktyki/MlotekDoKotletow/`

---

## 📂 Zawartość i Struktura Plików

| Plik | Opis |
| :--- | :--- |
| **[`MlotekDoKotletowItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/MlotekDoKotletow/MlotekDoKotletowItem.cs)** | Klasa broni ScriptableObject dziedzicząca po `WeaponItem`. Konfiguruje 45 pkt obrażeń bazowych (+5 więcej niż Zatruty Miecz), powolny zamach (cooldown 0.95s), koszt staminy przy LMP (7 pkt), 10% szansy na lekki odrzut/ogłuszenie oraz potężny atak PPM (+15 dmg, zerowanie staminy, spowolnienie gracza o 50% na 1.2s, odrzut 8.5m i ogłuszenie 1.2s). |
| **[`MlotekDoKotletowPickup.cs`](file:///g:/beergeon_v2/Assets/Praktyki/MlotekDoKotletow/MlotekDoKotletowPickup.cs)** | Komponent interaktywnego pickupa w świecie gry (podnoszenie klawiszem `[E]` z raycasta lub w promieniu 3.5m). |
| **[`MlotekDoKotletowTestHelper.cs`](file:///g:/beergeon_v2/Assets/Praktyki/MlotekDoKotletow/MlotekDoKotletowTestHelper.cs)** | Skrypt pomocniczy – naciśnięcie klawisza **`[M]`** w trybie gry natychmiast dodaje Młotek do ekwipunku gracza. |
| **`MlotekDoKotletow.png`** | Grafika tłuczka do kotletów (używana jako ikona UI, model trzymany w dłoni oraz sprite w świecie). |
| **`MlotekDoKotletow.asset`** | Skonfigurowany ScriptableObject broni (dostępny w `Assets/Praktyki/MlotekDoKotletow/` oraz `Assets/Resources/Items/`). |
| **`MlotekDoKotletowPickup.prefab`** | Prefab gotowej znajdźki w świecie gry z colliderem, rendererem i billboardem (w `Assets/Resources/Items/` oraz `Assets/Praktyki/MlotekDoKotletow/`). |
| **`MlotekDoKotletow_ShopItem.asset`** | Oferta handlarza NPC z ustaloną ceną **70 kapsli** (+10 kapsli więcej niż miecz, w `Assets/Resources/ShopItems/` oraz `Assets/Praktyki/MlotekDoKotletow/`). |

---

## ⚙️ Wprowadzone Integracje w Silniku Gry

1. **Sterowanie i Zużycie Staminy ([`PlayerStamina.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sprint/PlayerStamina.cs))**:
   - Dodano metody:
     - `DrainStamina(float amount)` – pobiera określoną liczbę punktów przy ciosie bronią i odnawia timer opóźnienia regeneracji.
     - `DrainAllStamina(float extraRegenDelay)` – natychmiast zeruje staminę do 0 i wydłuża chwilowo pauzę regeneracji.
     - `HasStaminaPercent(float minPercent)` – sprawdza, czy gracz posiada wymagany procent staminy (np. 90%).

2. **Tymczasowe Spowolnienie Gracza ([`PlayerController.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerController.cs))**:
   - Dodano metodę `ApplyTemporarySlow(float multiplier, float duration)`.
   - Ruch gracza w `Update()` uwzględnia tymczasowy mnożnik prędkości (50% prędkości przez 1.2 sekundy po silnym zamachu młotem).

3. **Mechanika Odrzutu i Ogłuszenia Przeciwników ([`EnemyController.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyController.cs))**:
   - Dodano wektor pędu odrzutu `knockbackVelocity` oraz stoper ogłuszenia `stunTimer`.
   - Metoda `ApplyKnockback(Vector3 direction, float force, float stunDuration)` odrzuca wroga fizycznie w tył, a na czas ogłuszenia blokuje poruszanie się i atakowanie gracza.

4. **Obsługa Ciężkiego Ataku i Cooldownu ([`PlayerHand.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHand.cs))**:
   - `OnAttack` uwzględnia indywidualny czas odnowienia przedmiotu (`currentItem.cooldown`).
   - Dodano metody `TriggerCustomAttack` oraz `HeavySwingRoutine` dla płynnego, ciężkiego zamachu z góry.
   - Uelastyczniono metodę `PerformRaycast` z obsługą niestandardowych obrażeń i callbacków trafienia.

5. **System Dropu z Przeciwników ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs))**:
   - Dodano pole `mlotekDoKotletowDropChance = 0.02f` (**2% szansy** na drop z pokonanego Hmielnika, identycznie jak Zatruty Miecz).

6. **Oferta w Sklepie Handlarza ([`ShopManagerUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Shop/ShopManagerUI.cs))**:
   - Plik `MlotekDoKotletow_ShopItem.asset` umieszczony w `Assets/Resources/ShopItems/` jest automatycznie ładowany do asortymentu sklepu z ceną **70 kapsli**.

---

## 📊 Balans Broni

| Cecha | Zatruty Miecz | Młotek Do Kotletów (LMP) | Młotek Do Kotletów (PPM - Rozbijacz) |
| :--- | :--- | :--- | :--- |
| **Typ ataku** | Szybkie cięcie punktowe | Ciężki zamach obuchowy | Miażdżący cios obszarowy z góry |
| **Obrażenia bezpośrednie** | 40 pkt | **45 pkt** (przy staminie < 10%: **9 pkt / 20%**) | **60 pkt** (45 + 15) |
| **Szybkość / Cooldown** | 0.4s (błyskawiczny) | **1.35s** (odczuwalnie ciężki i wolny) | **2.4s** (masywny, powolny zamach) |
| **Zasięg** | 4.0 m | **3.5 m** | **3.5 m** (stożek 75° przed graczem) |
| **Koszt Staminy** | 0 pkt | **7 pkt** (przy staminie < 10%: **0 pkt**) | **Wymaga $\ge 90\%$ $\rightarrow$ spada do 0** |
| **Kara dla gracza** | Brak | Brak | **-50% prędkości ruchu przez 1.2s** |
| **Wpływ na wroga** | 35% na zatrucie (15 DoT) | **10% szansy: lekki odrzut (3.5) + ogłuszenie (0.4s)** (brak przy <10% staminy) | **Gwarantowane: potężny odrzut (8.5) + ogłuszenie (1.2s)** |
| **Cena w sklepie** | 60 kapsli | — | **70 kapsli** |
| **Szansa na drop** | 2% | — | **2%** |

---

## 🎮 Jak Przetestować w Unity

### Sposób 1: Klawisz Szybkiego Testu [M]
1. Dodaj skrypt [`MlotekDoKotletowTestHelper.cs`](file:///g:/beergeon_v2/Assets/Praktyki/MlotekDoKotletow/MlotekDoKotletowTestHelper.cs) do dowolnego obiektu w scenie (lub gracza `Player`).
2. Wejdź w **Play Mode** i wciśnij klawisz **`[M]`**.
3. Młotek natychmiast trafi do Twojego ekwipunku!

### Sposób 2: Podniesienie ze Świata Gry
1. Przeciągnij prefab `Assets/Resources/Items/MlotekDoKotletowPickup.prefab` na scenę przed gracza.
2. Uruchom grę, podejdź do młotka i naciśnij klawisz **`[E]`**.

### Sposób 3: Zakup w Sklepie
1. Podejdź do Handlarza NPC i otwórz sklep klawiszem **`[E]`**.
2. Na liście przedmiotów pojawi się **Młotek Do Kotletów** w cenie **70 kapsli**.

### Sposób 4: Testowanie Walki
1. **LMP (Zwykły atak)**:
   - Zauważ powolniejszy zamach młota.
   - Pasek staminy lekko uszczupla się o 7 pkt przy każdym uderzeniu.
   - Przy trafieniu wroga zadaje 45 DMG (+ skalowanie z siłą gracza), z 10% szansą na odrzut w tył i mikro-ogłuszenie.
2. **PPM (Rozbijacz Kotletów)**:
   - Upewnij się, że masz prawie pełny pasek staminy ($\ge 90\%$).
   - Wciśnij **PPM**:
     - Młot wykona potężny zamach z góry.
     - Pasek staminy spadnie natychmiast do 0.
     - Gracz przez 1.2 sekundy porusza się o połowę wolniej.
     - Wrogowie przed Tobą otrzymają **60 pkt obrażeń**, odlecą na odległość kilku metrów i zostaną ogłuszeni na 1.2 sekundy!
