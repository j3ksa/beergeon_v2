# Włócznia — Dokumentacja Techniczna i Instrukcja (Praktyki)

## 1. Przegląd i Koncepcja
Włócznia to broń drzewcowa dalekiego zasięgu (4.8m) zaprojektowana z unikalnymi mechanikami pozycjonowania (tzw. **Sweet Spot**) oraz aktywną mobilnością w postaci **Szarży (Charge)**.

- **Kluczowy zamysł bojowy**: Nagradza gracza za zachowanie dystansu. Trafienie samym ostrzem włóczni zadaje potężne obrażenia, podczas gdy wróg, który podejdzie zbyt blisko, otrzyma zredukowane obrażenia od trzonka.
- **Szarża (PPM)**: Pozwala na szybkie wbijanie się w grupę wrogów z potężnym odrzutem pierwszego celu i zepchnięciem drugiego, jednak kolizja z przeszkodami otoczenia grozi utratą równowagi, utratą staminy i obrażeniami własnymi.
- **Pasywne obciążenie**: Ze względu na gabaryty włóczni, poruszanie się z nią w dłoni spowalnia gracza o 12%.

---

## 2. Statystyki Broni
| Parametr | Wartość | Opis |
| :--- | :--- | :--- |
| **Obrażenia bazowe** | `22` | Bazowe obrażenia (skalowane siłą gracza i kowadłem) |
| **Zasięg pchnięcia** | `4.8m` | Ponad dwukrotnie większy niż standardowa broń biała (2.0m) |
| **Strefa Precyzji (Sweet Spot)** | `3.2m - 4.8m` | **+25% obrażeń** przy trafieniu ostrzem |
| **Kara za bliski dystans** | `< 3.0m` | **-50% obrażeń** (uderzenie trzonkiem) |
| **Szybkość ataku (cooldown)** | `0.55s` | Atak oparty na pchnięciu w przód |
| **Szarża (PPM) - Prędkość** | `16 m/s` | Błyskawiczny doskok w przód |
| **Szarża - Koszt staminy** | `15 pkt` | Wymagana minimalna ilość staminy do startu |
| **Szarża - Uderzenie w cel 1** | `160% dmg` | Pełne obrażenia szarży + silny odrzut w przód |
| **Szarża - Uderzenie w cel 2** | `80% dmg` | 50% obrażeń szarży + odepchnięcie na bok i zatrzymanie szarży |
| **Szarża - Uderzenie w ścianę** | `-5 HP, -15 staminy` | Utrata równowagi po uderzeniu w ścianę |
| **Szarża - Anulowanie** | `PPM` | Ponowne kliknięcie PPM natychmiast bezpiecznie zatrzymuje szarżę |
| **Spowolnienie gracza** | `-12%` | `WeaponSpeedModifier = 0.88` w dłoni |
| **Szansa na krytyk** | `8%` (mnożnik `1.6x`) | Kompatybilne z bonusem mistrzowskim kowadła (+15%) |
| **Cena w sklepie** | `50 kapsli` | `Wlocznia_ShopItem.asset` |
| **Szansa na drop z wrogów** | `4%` | Konfigurowalne w `EnemyHealth.cs` |

---

## 3. Utworzone i Zmodyfikowane Pliki
1. [WloczniaItem.cs](file:///g:/beergeon_v2/Assets/Praktyki/W%C5%82%C3%B3cznia/WloczniaItem.cs) — główna klasa broni dziedzicząca po `WeaponItem`.
2. [WloczniaPickup.cs](file:///g:/beergeon_v2/Assets/Praktyki/W%C5%82%C3%B3cznia/WloczniaPickup.cs) — skrypt podnoszenia klawiszem `[E]` (`IInteractable`).
3. [WloczniaPickup.prefab](file:///g:/beergeon_v2/Assets/Praktyki/W%C5%82%C3%B3cznia/WloczniaPickup.prefab) — prefab przedmiotu leżącego w świecie gry.
4. [Wlocznia.asset](file:///g:/beergeon_v2/Assets/Praktyki/W%C5%82%C3%B3cznia/Wlocznia.asset) — ScriptableObject przedmiotu w folderze praktyk oraz w `Assets/Resources/Items/Wlocznia.asset`.
5. [Wlocznia_ShopItem.asset](file:///g:/beergeon_v2/Assets/Praktyki/Shop/Wlocznia_ShopItem.asset) — oferta sklepowa za 50 kapsli w `Assets/Praktyki/Shop/` oraz `Assets/Resources/ShopItems/`.
6. [EnemyHealth.cs](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs) — dodano szansę na drop (4%) przy śmierci przeciwnika.
7. [PlayerHand.cs](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHand.cs) — obsługa pchnięcia `SpearThrustRoutine` i odblokowanie PPM dla broni funkcyjnych.
