# 🗡️ Poradnik Implementacji: Zatruty Miecz (Poison Sword)

Kompletny system dla **Zatrutego Miecza** (broń biała zadająca 40 obrażeń bezpośrednich, 35% szansy na nałożenie trucizny DoT zadającej 3 pkt obrażeń co 1s przez 5s, jadowicie zielony efekt wizualny na wrogu, 2% szansy na drop z przeciwników oraz oferta w sklepie za 60 kapsli) został w pełni zaimplementowany i zintegrowany z projektem.

Folder główny: `Assets/Praktyki/ZatrutyMiecz/`

---

## 📂 Zawartość i Struktura Plików

| Plik | Opis |
| :--- | :--- |
| **[`ZatrutyMieczItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/ZatrutyMiecz/ZatrutyMieczItem.cs)** | Klasa broni ScriptableObject dziedzicząca po `WeaponItem`. Konfiguruje 40 obrażeń bezpośrednich (+5 więcej niż kij bejsbolowy), zasięg 4, animację machnięcia w dłoni oraz nadpisuje metodę `OnHit` z 35% szansą na nałożenie trucizny. |
| **[`PoisonStatusEffect.cs`](file:///g:/beergeon_v2/Assets/Praktyki/ZatrutyMiecz/PoisonStatusEffect.cs)** | Komponent nakładany dynamicznie na trafionego przeciwnika (`AddComponent<PoisonStatusEffect>()`). Zarządza cyklicznym zadawaniem obrażeń DoT (`IDamageable.TakeDamage`), jadowicie zielonym zabarwieniem sprite'a, odświeżaniem czasu przy kolejnych ciosach i bezpiecznym zdejmowaniem efektu po wygaśnięciu lub śmierci. |
| **`ZatrutyMiecz.png`** | Grafika miecza z zieloną klingą (wykorzystywana jako ikona w ekwipunku, model trzymany w dłoni oraz sprite znajdźki w świecie). |
| **`ZatrutyMiecz.asset`** | Skonfigurowany plik ScriptableObject broni z bazowymi obrażeniami, szansą i parametrami trucizny. Dostępny w `Assets/Praktyki/ZatrutyMiecz/` oraz `Assets/Resources/Items/`. |
| **`ZatrutyMieczPickup.prefab`** | Prefab znajdźki w świecie gry (zawiera BoxCollider, SpriteRenderer, `ItemPickup` oraz `Billboard` obracający się do kamery). |
| **`ZatrutyMiecz_ShopItem.asset`** | Przedmiot sklepowy handlarza NPC z ustaloną ceną **60 kapsli** (w `Assets/Resources/ShopItems/` oraz `Assets/Praktyki/Shop/`). |

---

## ⚙️ Wprowadzone Integracje w Kodzie Gry

1. **Baza Broni ([`WeaponItem.cs`](file:///g:/beergeon_v2/Assets/Scripts/Inventory/WeaponItem.cs))**:
   - Dodano wirtualną metodę:
     ```csharp
     public virtual void OnHit(GameObject target, IDamageable damageable, RaycastHit hit) { }
     ```
     dzięki czemu każda wyspecjalizowana broń może definiować własne efekty przy trafieniu w cel.

2. **Obsługa Ciosu w Dłoni Gracza ([`PlayerHand.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHand.cs))**:
   - W metodzie `PerformRaycast()` dodano wyszukiwanie `IDamageable` w obiekcie nadrzędnym (`GetComponentInParent`), co gwarantuje poprawne wykrywanie potworów z wieloma colliderami.
   - Dodano automatyczne wywołanie `currentWeapon.OnHit(...)` po zadaniu obrażeń bezpośrednich.

3. **System Dropu z Przeciwników ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs) & [`Hmielnik.prefab`](file:///g:/beergeon_v2/Assets/Prefabs/Hmielnik.prefab))**:
   - Dodano pola `zatrutyMieczPrefab` oraz `zatrutyMieczDropChance = 0.02f` (**2% szansy**).
   - W metodzie `Die()` po zgonie wroga następuje rzut na drop z automatycznym fallbackiem do `Resources.Load<GameObject>("Items/ZatrutyMieczPickup")`.

4. **Sklep Handlarza NPC ([`ShopManagerUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Shop/ShopManagerUI.cs))**:
   - `ZatrutyMiecz_ShopItem.asset` umieszczony w folderze `Assets/Resources/ShopItems/` jest automatycznie ładowany przez `ShopManagerUI.Start()` do puli oferowanych towarów za **60 kapsli**.

5. **Zgodność z Systemem Zapisu Gry ([`SaveManager.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SaveSystem/SaveManager.cs))**:
   - Plik `ZatrutyMiecz.asset` znajduje się w `Assets/Resources/Items/`, dzięki czemu `SaveManager` i `PlayerSaveable` bez problemu zapisują i wczytują miecz z ekwipunku gracza.

---

## 📊 Balans i Działanie Trucizny (Hit & Run)

- **Obrażenia Bezpośrednie**: **40 pkt** (kij bejsbolowy ma 35 pkt).
- **Szansa na Zatrucie**: **35%** (suwak `[Range(0, 1)]` w Inspektorze). Statystycznie przy szybkim ataku co ~0.4s przeciwnik zostaje otruty w 2–3 ciosie.
- **Obrażenia Trucizny**: **3 pkt co 1s przez 5s** (łącznie 15 pkt).
  - Standardowy przeciwnik (**Hmielnik**) posiada **50 HP**.
  - 1 cios mieczem (40 dmg) $\rightarrow$ wrogowi zostaje **10 HP**.
  - Trucizna:
    - 1 sekunda: 10 HP - 3 = 7 HP
    - 2 sekunda: 7 HP - 3 = 4 HP
    - 3 sekunda: 4 HP - 3 = 1 HP
    - 4 sekunda: 1 HP - 3 $\le$ 0 HP $\rightarrow$ **Przeciwnik sam ginie od trucizny!**
  - Umożliwia to efektywną taktykę: jeden cios, nałożenie trucizny i odskok na bezpieczną odległość.
- **Odświeżanie Trucizny (Refresh)**: Kolejny celny cios z aktywacją trucizny odnawia licznik trwania do pełnych 5 sekund.
- **Efekt Wizualny**: Podczas zatrucia sprite potwora przyjmuje jadowity odcień zieleni, a przy każdym ticku obrażeń wywołuje czerwony flash obrażeń. Po wygaśnięciu trucizny kolory wracają do normy.

---

## 🎮 Jak Przetestować w Unity Editorze

### Sposób 1: Podniesienie Miecza w Świecie Gry
1. Przeciągnij prefab `Assets/Resources/Items/ZatrutyMieczPickup.prefab` lub `Assets/Praktyki/ZatrutyMiecz/ZatrutyMieczPickup.prefab` na scenę przed gracza.
2. Uruchom grę (**Play Mode**), podejdź do miecza i naciśnij klawisz **`[E]`** (działa zarówno przez wycelowanie celownikiem, jak i po prostu podejście w promieniu 2.5m).
3. Miecz natychmiast trafi do Twojego ekwipunku!

### Sposób 2: Natychmiastowy Test Klawiszem [Z]
1. Dodaj skrypt [`ZatrutyMieczTestHelper.cs`](file:///g:/beergeon_v2/Assets/Praktyki/ZatrutyMiecz/ZatrutyMieczTestHelper.cs) do dowolnego obiektu w scenie (lub gracza).
2. W trakcie gry wciśnij klawisz **`[Z]`** na klawiaturze – Zatruty Miecz pojawi się od razu w aktywnym ekwipunku!

### Sposób 3: Zakup u Handlarza NPC
1. Podejdź do NPC Handlarza i otwórz sklep klawiszem **`[E]`**.
2. W ofercie pojawi się **Zatruty Miecz** za **60 kapsli**.

### Sposób 4: Test Walki i Trucizny
1. Wyposaż Zatruty Miecz w dłoń (wybierz odpowiedni slot ekwipunku).
2. Zaatakuj przeciwnika (**LPM**).
3. Przy wejściu szansy (35%):
   - W konsoli pojawi się komunikat:
     `[Zatruty Miecz] Przeciwnik Hmielnik został ZATRUТY! (3 dmg co 1s przez 5s)`
   - Przeciwnik zabarwi się na zielono.
   - Co 1 sekundę przeciwnik otrzyma 3 pkt obrażeń, aż do wygaśnięcia efektu lub zgonu.
