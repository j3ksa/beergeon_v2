# Instrukcja Wdrożenia Mikstury "Małpka" (Pełna Konfiguracja)

Wszystkie pliki znajdują się w folderze `Assets/Praktyki/Malpka/`.

---

## KROK 1: Stworzenie Przedmiotu w Project (ScriptableObject)

1. W panelu **Project** przejdź do folderu `Assets/Resources/Items/` (lub `Assets/Praktyki/Malpka/`).
2. Kliknij prawym przyciskiem myszy: **`Create > Inventory > Potion`**.
3. Nazwij plik: **`MonkeyPotionItem`**.
4. W **Inspektorze** ustaw:
   - **Item Name**: `Małpka`
   - **Icon**: Przypisz grafikę `monkey.PNG`.
   - **Potion Type**: Wybierz **`Strength`**.
   - **Amount**: `1.5` *(lub 2.0 dla podwójnych obrażeń)*.
   - **Duration**: `10` *(czas trwania szału w sekundach)*.

---

## KROK 2: Obiekt na Mapie do Podnoszenia (Pod [E]) + Obracanie za Graczem

1. W panelu **Hierarchy** utwórz pusty obiekt (`Create Empty`), nazwij go **`MonkeyPotion_World`**.
2. Dodaj **`Sprite Renderer`** z grafiką `monkey.PNG`.
3. Dodaj **`Box Collider`** (zaznacz **`Is Trigger = true`**).
4. Dodaj skrypt **`Item Pickup`** (`ItemPickup.cs`):
   - W polu **Item** przeciągnij plik **`MonkeyPotionItem`** z Kroku 1.
5. Dodaj komponent **`Billboard`** (`Assets/Scripts/Systems/Billboard.cs`):
   - Dzięki temu mikstura na mapie **obraca się zawsze przodem do kamery gracza** (dokładnie tak jak Mocny Gul!).

---

## KROK 3: Dodanie Czerwonej Obwódki Szału (Vignette) na Ekranie

1. W panelu **Hierarchy** przejdź do swojego **Canvasa UI**.
2. Kliknij prawym przyciskiem: **`Create Empty`** i nazwij obiekt **`RageVignette`**.
3. Dodaj skrypt **`RageVignetteUI`** (`Assets/Praktyki/Malpka/RageVignetteUI.cs`).
