# Instrukcja Wdrożenia i Obsługi Systemu Zapisu (Save System)

Wszystkie pliki skryptów znajdują się w folderze `Assets/Praktyki/SaveSystem`.

---

## 1. Koncepcja i Architektura Systemu

System bazuje na zapisie danych do formatu **JSON** w ścieżce `Application.persistentDataPath` (`save_slot_1.json`, `save_slot_2.json`, itd.).
Dzięki temu pliki zapisu są odporne na aktualizacje gry, łatwe w debugowaniu i przenośne.

### Zapisywane Elementy:
1. **Gracz**:
   - Pozycja ($X, Y, Z$) oraz rotacja.
   - Poziom gracza (Level), Punkty Doświadczenia (XP).
   - Statystyki (`Strength`, `Vitality`).
2. **Przeciwnicy na Scenie**:
   - Unikalny identyfikator `SaveID` dla każdego przeciwnika.
   - Położenie i rotacja.
   - Punkty zdrowia.
   - Stan przeżycia (`isDead`) – pokonani przeciwnicy nie pojawiają się ponownie po wczytaniu.
3. **Środowisko / Meta**:
   - Aktywna nazwa sceny.
   - Czas utworzenia zapisu (data i godzina).

---

## 2. Instrukcja Krok po Kroku (Konfiguracja w Unity)

### Krok 1: Obiekt Menadżera Zapisu (`SaveManager`)
1. Stwórz na scenie pusty GameObject i nazwij go `SaveManager`.
2. Dodaj do niego skrypt `SaveManager.cs`.
3. Skrypt używa wzorca **Singleton** i `DontDestroyOnLoad`, dzięki czemu będzie działał automatycznie między scenami.

### Krok 2: Konfiguracja Gracza
1. Wybierz obiekt Gracza w hierarchii scene (`Player`).
2. Dodaj skrypt `SaveableEntity.cs`.
3. W Inspektorze kliknij w menu kontekstowym komponentu lub poczekaj na automatyczną generację `Save ID`.
4. Dodaj skrypt `PlayerSaveable.cs`. Komponent automatycznie powiąże się z `PlayerStats` oraz traformacją gracza.

### Krok 3: Konfiguracja Przeciwników
1. Na obiekcie Przeciwnika (np. Prefabie lub instancji na scenie) dodaj komponent `SaveableEntity.cs`.
2. Dodaj komponent `EnemySaveable.cs`.
3. Każda instancja przeciwnika na scenie powinna mieć unikalny `Save ID` (wygenerowany automatycznie w edytorze).

---

## 3. Konfiguracja Menu UI (Sloty Zapisu / Odczytu)

### Proponowany Układ w Canvasie:
1. Stwórz panel `SaveLoadPanel` w Canvasie menu głównego lub menu pauzy.
2. Dodaj komponent `SaveMenuUI.cs` do panelu `SaveLoadPanel`.
3. Stwórz 3 wiersze/sloty (np. `Slot_1`, `Slot_2`, `Slot_3`). Każdy slot powienien zawierać:
   - **Tekst (TextMeshPro)**: Wyświetlający nazwę slotu, poziom gracza, nazwę mapy oraz datę zapisu.
   - **Przycisk Zapisz**: Wywołujący `SaveMenuUI.OnClickSave(1)` (lub odpowiednio 2, 3).
   - **Przycisk Wczytaj**: Wywołujący `SaveMenuUI.OnClickLoad(1)`.
   - **Przycisk Usuń**: Wywołujący `SaveMenuUI.OnClickDelete(1)`.
4. Przeciągnij przyciski i teksty do odpowiednich tablic w inspektorze `SaveMenuUI`.

---

## 4. Przykładowe Wywołanie z Kodu

Jeśli chcesz wywołać zapis/wczytanie w własnym skrypcie lub przycisku:

```csharp
using SaveSystem;

// Zapis gry do slotu 1
SaveManager.Instance.SaveGame(1);

// Odczyt gry ze slotu 1
SaveManager.Instance.LoadGame(1);
```
