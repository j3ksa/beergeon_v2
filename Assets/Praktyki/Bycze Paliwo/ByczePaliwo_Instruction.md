# ⚡ Poradnik Implementacji: Energetyk "Bycze Paliwo" (Bull Fuel)

Cześć! Kompletny system dla nowego energetyka **"Bycze Paliwo"** (mikstury natychmiastowo uzupełniającej staminę do 100%) został w pełni zaimplementowany w folderze:
`Assets/Praktyki/Bycze Paliwo/`

---

## 📂 Zawartość folderu `Assets/Praktyki/Bycze Paliwo/`

| Plik | Opis |
| :--- | :--- |
| **[`ByczePaliwoItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Bycze%20Paliwo/ByczePaliwoItem.cs)** | Klasa przedmiotu (ScriptableObject dziedziczący po `EquippableItem`), obsługująca picie z ekwipunku i natychmiastowe uzupełnienie staminy na 100%. |
| **[`ByczePaliwoPotion.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Bycze%20Paliwo/ByczePaliwoPotion.cs)** | Skrypt na obiekt w świecie gry (World Pickup z kolizją trigger, automatycznym obracaniem `Billboard` do kamery oraz magnesem do gracza). |
| **[`ByczePaliwoUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Bycze%20Paliwo/ByczePaliwoUI.cs)** | Skrypt efektu wizualnego UI – generuje dynamiczny, złocisto-żółty energetyczny błysk (winietę) na krawędziach ekranu po wypiciu/zebraniu. |
| **`ByczePaliwo.png`** | Gotowa, wygenerowana piksel-artowa grafika puszki Byczego Paliwa z motywem byka i błyskawic. |
| **`ByczePaliwo.asset`** | Gotowy i skonfigurowany plik ScriptableObject przedmiotu z przypisaną ikoną i opisem. |
| **`ByczePaliwo_ShopItem.asset`** | Gotowy plik towaru do sklepu u handlarza NPC (cena: 12 kapsli). |

---

## 🚀 Instrukcja Krok po Kroku

### KROK 1: Przedmiot w Ekwipunku (ScriptableObject)
Plik **`ByczePaliwo.asset`** jest już w pełni przygotowany w folderze `Assets/Praktyki/Bycze Paliwo/`.
- Jeśli chcesz stworzyć kolejną wersję w Unity:
  - Kliknij prawym przyciskiem w Project $\rightarrow$ **Create** $\rightarrow$ **Inventory** $\rightarrow$ **Bycze Paliwo** (lub **Potion** z typem `Stamina`).
  - Ustaw nazwę: `Bycze Paliwo`, przypisz ikonę `ByczePaliwo.png` i upewnij się, że opcja **Instant Full Stamina** jest zaznaczona (`true`).

---

### KROK 2: Dodanie Byczego Paliwa do Sklepu Handlarza NPC
Plik **`ByczePaliwo_ShopItem.asset`** jest już gotowy!
1. W Unity zaznacz obiekt **ShopManager / Handlarz NPC** w scenie (lub Prefab `PauseMenu` / `ShopManagerUI`).
2. W komponencie **`ShopManagerUI`** odszukaj pole **`Item Pool`**.
3. Przeciągnij plik **`ByczePaliwo_ShopItem`** do listy `Item Pool`.
4. Gotowe! Teraz Bycze Paliwo będzie pojawiać się w ofercie u handlarza w grze.

---

### KROK 3: Dodanie Efektu Błysku na Ekranie (ByczePaliwoUI)
1. W panelu **Hierarchy** wybierz swój główny obiekt **Canvas** (lub stwórz pusty obiekt `Create Empty` w Canvasie o nazwie np. `ByczePaliwo_UI`).
2. Dodaj do niego komponent **`ByczePaliwoUI`** (`Assets/Praktyki/Bycze Paliwo/ByczePaliwoUI.cs`).
3. *Wskazówka:* Skrypt nie wymaga podpina żadnych tekstur – sam wygeneruje miękką, proceduralną winietę i zniknie na starcie, a rozbłyśnie na złoto w momencie wypicia puszki!

---

### KROK 4: Umieszczenie Energetyka na Mapie jako Znajdźki (World Pickup)
Aby puszka leżała fizycznie w lochu / na mapie:
1. W panelu **Hierarchy** utwórz obiekt: **`Create Empty`** $\rightarrow$ nazwij go **`ByczePaliwo_World`**.
2. Dodaj komponent **`Sprite Renderer`** i wybierz sprite **`ByczePaliwo`**.
3. Dodaj komponent **`Box Collider`** (lub `Sphere Collider`) i zaznacz **`Is Trigger = true`**.
4. Dodaj skrypt **`ByczePaliwoPotion`** (`Assets/Praktyki/Bycze Paliwo/ByczePaliwoPotion.cs`).
   - Skrypt automatycznie doda komponent **`Billboard`**, dzięki czemu puszka zawsze będzie zwrócona przodem do kamery gracza (dokładnie tak jak Mocny Gul i Małpka)!

---

## 🎮 KROK 5: Testowanie w Grze

1. Uruchom grę w Unity Editorze.
2. Zmęcz postać biegając na sprincie (przytrzymaj **Shift**), aż pasek staminy spadnie do zera.
3. Wypij **Bycze Paliwo** (z ręki/ekwipunku) lub podbiegnij do leżącej na mapie puszki:
   - Pasek staminy natychmiast uzupełni się do **100%**!
   - Na ekranie pojawi się energetyczny złoty rozbłysk.
   - W konsoli Unity wyświetli się komunikat:
     `[Bycze Paliwo] Gracz wypił Bycze Paliwo! Stamina natychmiastowo odnowiona na MAX (100%)!`
