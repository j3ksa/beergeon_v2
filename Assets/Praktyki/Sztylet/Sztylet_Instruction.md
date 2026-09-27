# 🗡️ Poradnik Implementacji: Sztylet (Nóż Sprężynowy)

Kompletny system dla **Sztyletu** został w pełni zaimplementowany i zintegrowany z projektem.

Folder główny: `Assets/Praktyki/Sztylet/`

---

## 📂 Struktura Plików i Assetów

| Plik | Opis |
| :--- | :--- |
| **[`SztyletItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/SztyletItem.cs)** | Klasa broni dziedzicząca po `WeaponItem`. Konfiguruje 12 obrażeń bazowych, cooldown 0.24s (+40% szybkości), +25% do prędkości ruchu gracza, -10% zużycia staminy przy sprincie, 10% szansy na trafienie krytyczne (2x dmg), 10% szansy na krwawienie oraz system serii uderzeń (Combo). |
| **[`BleedStatusEffect.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/BleedStatusEffect.cs)** | Komponent nakładany dynamicznie na przeciwnika (`AddComponent<BleedStatusEffect>()`). Zadaje cykliczne obrażenia krwawienia DoT (1 pkt co 0.5s przez 3s) oraz barwi sprite przeciwnika na krwistoczerwony kolor, bezpiecznie przywracając oryginalne kolory po wygaśnięciu efektu lub śmierci. |
| **[`SztyletPickup.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/SztyletPickup.cs)** | Komponent obsługujący interaktywne podnoszenie sztyletu klawiszem `[E]` (`IInteractable`). |
| **[`Sztylet.png`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/Sztylet.png)** | Przezroczysta tekstura sprite sztyletu w stylu gry (gruby kontur, komiksowe cieniowanie). |
| **[`Sztylet.asset`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/Sztylet.asset)** | Skonfigurowany ScriptableObject broni. Zduplikowany również do `Assets/Resources/Items/Sztylet.asset` dla systemu zapisu gry (`SaveManager`) i automatycznego wczytywania. |
| **[`SztyletPickup.prefab`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/SztyletPickup.prefab)** | Prefab znajdźki w świecie (BoxCollider, SpriteRenderer, Billboard, SztyletPickup). Zduplikowany do `Assets/Resources/Items/SztyletPickup.prefab`. |
| **[`Sztylet_ShopItem.asset`](file:///g:/beergeon_v2/Assets/Praktyki/Shop/Sztylet_ShopItem.asset)** | Oferta sklepowa u Handlarza NPC za **35 kapsli**. Zduplikowany również do `Assets/Resources/ShopItems/Sztylet_ShopItem.asset`. |

---

## ⚙️ Wprowadzone Modyfikacje w Rdzennych Systemach

1. **Cykl życia ekwipunku ([`EquippableItem.cs`](file:///g:/beergeon_v2/Assets/Scripts/Inventory/EquippableItem.cs) & [`PlayerHand.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHand.cs))**:
   - Dodano wirtualne metody `OnEquipped(player, hand)` i `OnUnequipped(player, hand)`.
   - `PlayerHand` powiadamia trzymany przedmiot o wyciągnięciu do ręki lub schowaniu, co pozwala sztyletowi natychmiast aplikować bonusy do prędkości i staminy.

2. **Prędkość ruchu i sprint ([`PlayerController.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerController.cs) & [`PlayerStamina.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sprint/PlayerStamina.cs))**:
   - `PlayerController` uwzględnia mnożnik `WeaponSpeedModifier` (przy sztylecie: `1.25x` = +25% prędkości ruchu).
   - `PlayerStamina` uwzględnia `StaminaDrainMultiplier` (przy sztylecie: `0.90x` = staminę zużywasz o 10% wolniej podczas sprintu).

3. **System Dropu z Przeciwników ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs))**:
   - Dodano pola `sztyletPrefab` oraz `sztyletDropChance = 0.10f` (**10% szansy na drop** po śmierci wroga).
   - W metodzie `Die()` następuje rzut kością i zrzut `SztyletPickup` z fallbackiem z folderu Resources.

4. **Kowadło Ulepszeń ([`WeaponUpgradeManager.cs`](file:///g:/beergeon_v2/Assets/Praktyki/KowadloUlepszen/WeaponUpgradeManager.cs))**:
   - Sztylet dziedziczy po `WeaponItem`, dzięki czemu jest w 100% automatycznie wspierany przez kowadło (zwiększanie obrażeń na poziomach +1 do +6 oraz bonus mistrzowski +15% do szansy na krytyk na poziomie +6).

5. **Sklep Handlarza ([`ShopManagerUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Shop/ShopManagerUI.cs))**:
   - Przedmiot `Sztylet_ShopItem.asset` umieszczony w `Assets/Resources/ShopItems/` jest automatycznie ładowany do oferty sklepu za **35 kapsli**.
