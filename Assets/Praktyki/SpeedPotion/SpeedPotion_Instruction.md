# 🧪 Poradnik Krok po Kroku: Implementacja Speed Potion & Dodanie do Sklepu

Cześć! Skrypty C# dla mikstury przyspieszenia zostały już stworzone w ścieżce `Assets/Praktyki/SpeedPotion/`.

Poniżej znajdziesz instrukcję krok po kroku, jak przygotować obiekt w Unity Editorze, podpiąć grafiki oraz dodać potkę do sklepu u handlarza NPC!

---

## 📂 Pliki w projekcie (stworzone skrypty)
1. **[`SpeedPotionItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SpeedPotion/SpeedPotionItem.cs)** – Tworzy nowy typ przedmiotu mikstury (ScriptableObject) dziedziczący po `EquippableItem`.
2. **[`PlayerSpeedBuff.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SpeedPotion/PlayerSpeedBuff.cs)** – Komponent na graczu, który płynnie przelicza mnożnik prędkości, zarządza czasem trwania (timerem) i przywraca oryginalną prędkość po wygaśnięciu buffa.

---

## 🎨 KROK 1: Stworzenie Obiektu Mikstury (ScriptableObject) w Unity

Gdy będziesz miał gotowe grafiki (lub tymczasową ikonkę):

1. W oknie **Project** w Unity przejdź do folderu `Assets/Praktyki/SpeedPotion/`.
2. Kliknij prawym przyciskiem myszy w wolne miejsce $\rightarrow$ **Create** $\rightarrow$ **Inventory** $\rightarrow$ **Speed Potion** (lub **Potion**).
3. Nazwij stworzony plik np. `SpeedPotion_Item`.
4. W panelu **Inspector** ustaw parametry:
   - **Item Name**: `Jägerek` (lub `Napój Przyspieszenia`)
   - **Icon**: Przeciągnij gotowy Sprite / ikony mikstury.
   - **Duration**: `6` (zmieniono na 6 sekund!).

---

## 🌬️ KROK 1.5: Dodanie Efektu Wiatru (Niebieskie Pulsowanie na Ekranie)

**UPDATE:** Poprawiłem kod tak, aby działał idealnie w ten sam sposób co efekt od mikstury Małpki! 

Teraz nie musisz w ogóle ręcznie tworzyć żadnych obrazków ani podpinać grafik!
Wystarczy, że:

1. Przeciągniesz skrypt **[`SpeedBuffUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/SpeedPotion/SpeedBuffUI.cs)** z folderu `SpeedPotion` wprost na swój **Canvas** (lub jakikolwiek pusty obiekt w scenie).
2. Skrypt automatycznie podepnie się pod ekran, wygeneruje proceduralną, płynną teksturę "wiatru" na krawędziach i będzie nasłuchiwał, kiedy gracz wypije potkę.
3. To wszystko! Reszta dzieje się w kodzie. Znika od razu przy starcie i pojawia się z pięknym efektem pulsowania, gdy wypijesz miksturę.

---

## 🛒 KROK 2: Dodanie Mikstury do Sklepu NPC

Aby miksturę można było kupić u handlarza w sklepie:

1. W folderze `Assets/Praktyki/Shop/` (lub `SpeedPotion/`) kliknij PRAWYM przyciskiem $\rightarrow$ **Create** $\rightarrow$ **Shop** $\rightarrow$ **Shop Item**.
2. Nazwij stworzony plik np. `ShopItem_SpeedPotion`.
3. W **Inspectorze**:
   - **Item Data**: Przeciągnij stworzony wcześniej plik `SpeedPotion_Item` z Kroku 1.
   - **Cost**: Ustaw cenę w kapslach (np. `25`).
4. Otwórz obiekt handlarza NPC w scenie (lub SO listy przedmiotów handlarza) i dodaj `ShopItem_SpeedPotion` do listy oferowanych towarów.

---

## 🧪 KROK 3: Testowanie w Grze

1. Uruchom scenę.
2. Podejdź do Handlarza NPC i otwórz Sklep.
3. Kup **Speed Potion** za kapsle.
4. Otwórz ekwipunek i użyj mikstury (lub wybierz ją na pasek szybki i kliknij użycie).
5. Obserwuj postać – gracz porusza się zauważalnie szybciej przez 10 sekund, po czym prędkość wraca do normy! W konsoli Unity zobaczysz komunikat z logami.
