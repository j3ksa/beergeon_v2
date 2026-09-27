# Instrukcja konfiguracji: System Ulepszeń Statystyk (Stats Upgrade)

System znajduje się w folderze `Assets/Praktyki/StatsUpgrade`.

---

## 1. Podpięcie komponentu do Obiektu Gracza (Player)

1. W edytorze Unity znajdź w **Hierarchy** obiekt swojego Gracza (np. `Player`).
2. Kliknij **Add Component** w panelu **Inspector**.
3. Wyszukaj i dodaj skrypt **`PlayerStatsUpgrade`**.

### Konfiguracja pól w Inspectorze:
- **Składniki Gracza** (`PlayerStats`, `PlayerHealth`, `PlayerStamina`, `PlayerController`):
  - Jeśli skrypty te znajdują się na tym samym obiekcie `Player`, skrypt wykryje je **automatycznie** po uruchomieniu gry.
  - Możesz też przeciągnąć je ręcznie z Inspectora.

- **Punkty Statystyk**:
  - `Available Points` – liczba aktualnie posiadanych punktów do wydania (np. 0 na start).
  - `Points Per Level` – ile punktów gracz otrzymuje po każdym awansie poziomu (domyślnie `2`).

- **Wartości Ulepszeń na 1 Punkt (Wartości zbalansowania)**:
  - `Health Per Point`: domyślnie `15` HP.
  - `Stamina Per Point`: domyślnie `10` staminy.
  - `Stamina Regen Per Point`: domyślnie `2.0` staminy/sek.
  - `Speed Per Point`: domyślnie `0.5` prędkości ruchu.
  - `Strength Per Point`: domyślnie `2` do siły.
  - `Crit Chance Per Point`: domyślnie `5%` szansy na trafienie krytyczne.

- **Maksymalne Poziomy Ulepszeń**:
  - Pozwalają ustalić limity ulepszeń (np. maksymalnie 10 poziomów dla szansy na krytyk = 50%).

---

## 2. Podpięcie Interfejsu UI

1. Dodaj pusty obiekt w swoim Canvasie lub stwórz nowy **UI -> Panel** i nazwij go np. `StatsUpgradePanel`.
2. Dodaj komponent **`StatsUpgradeUI`** do tego panelu lub do Canvasu.
3. Przypisz pola tekstowe (**TextMeshProUGUI**) i przyciski (**Button**) do odpowiednich pól w Inspectorze skryptu `StatsUpgradeUI`.
4. Ustaw klawisz otwierania panelu w polu `Toggle Key` (domyślnie `U`).

---

## 3. Zapis Stanu Gry

Skrypt `PlayerSaveable` automatycznie zapisuje unspent points oraz ulepszenia w pliku zapisu gry `SaveData`.
