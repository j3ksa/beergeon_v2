# Instrukcja Wdrożenia Minigry BlackJack (Oczko z Hazardzistą)

Wszystkie skrypty znajdują się w folderze `Assets/Praktyki/BlackJack/`.

System wspiera:
- **Proceduralne karty** – nie musisz tworzyć 52 grafik kart! Karta sama renderuje figury (A, K, Q, J, 10..2), kolory (♠, ♥, ♦, ♣) oraz barwę (czerwony/czarny), a także rewers.
- **Zakłady na kapsle** – integracja z `PlayerCurrency` (`+5`, `+10`, `+25`, `+50`, `+100`, `Clear`, `All-In`, podwajanie stawki `Double Down`).
- **Inteligentnego bota hazardzistę** – gra zgodnie z zasadami kasyna (dobiera do 17, pasuje od 17), ma unikalne teksty dialogowe i reakcje.
- **Interakcję z postacią** – działa na kliknięcie lewym przyciskiem myszy w postać lub podejście i wciśnięcie klawisza `[E]`.

---

## NAJSZYBSZY SPOSÓB (1 KLIKNIĘCIE W UNITY):

Przygotowałem dla Ciebie automatyczne narzędzie w górnym pasku Unity:

1. **Podpięcie Postaci Hazardzisty**:
   - Zaznacz w oknie **Hierarchy** postać swojego hazardzisty.
   - W górnym menu Unity kliknij: **`Tools > BlackJack > 1. Podepnij Hazardziste pod Zaznaczony Obiekt`**.
   - Skrypt sam doda Collider i komponent `GamblerNPC`!

2. **Stworzenie Okna Gry w Canvasie**:
   - W górnym menu Unity kliknij: **`Tools > BlackJack > 2. Stworz Kompletne Okno BlackJack w Canvas`**.
   - Gotowe! Skrypt sam wygeneruje całe zielone sukno stołu, przyciski zakładów (+5, +10, +25, +50, +100, All-In, Rozdaj), przyciski gry (Dobierz, Pasuj, Podwój), liczniki punktów, dymek bota i podepnie wszystko pod `BlackJackUI`.

---

## SPOSÓB RĘCZNY (KROK PO KROKU):

### KROK 1: Podpięcie Skryptu pod Postać Hazardzisty

1. W oknie **Hierarchy** zaznacz obiekt swojej postaci hazardzisty (np. `GamblerNPC` / model / sprite).
2. Upewnij się, że obiekt posiada **Collider** (np. `Capsule Collider`, `Box Collider` lub `Sphere Collider`), aby myszka mogła w niego trafić.
3. Kliknij **Add Component** i dodaj skrypt **`GamblerNPC`** (`Assets/Praktyki/BlackJack/GamblerNPC.cs`).
4. W Inspektorze:
   - **Interaction Radius**: domyślnie `5.47` (odległość, z której gracz może wejść w interakcję).
   - **Allow Mouse Click**: zaznaczone `true` (umożliwia kliknięcie myszą).
   - **Allow Key Interaction**: zaznaczone `true` (umożliwia wciśnięcie `[E]`).
   - *(Opcjonalnie)* Jeśli Twoja postać ma dymek/Canvas z napisem `[E] Rozmawiaj`, możesz go przeciągnąć do pola **Interaction Prompt UI**.

---

## KROK 2: Stworzenie Okna Gry w Canvasie UI

1. W **Hierarchy** znajdź swój główny **Canvas** (ten sam, w którym jest np. sklep lub pauza gry).
2. Stwórz pod nim pusty obiekt: prawy przycisk myszy na Canvas -> **Create Empty**, nazwij go **`BlackJackWindow`**.
3. Dodaj do niego komponent **`BlackJackUI`** (`Assets/Praktyki/BlackJack/BlackJackUI.cs`).
   *(Komponent `BlackJackGame` doda się automatycznie!)*
4. W Inspektorze `BlackJackUI`:
   - W polu **Window Root** przeciągnij ten obiekt `BlackJackWindow`.
   - Stwórz w nim tło (np. **UI > Image**, ciemny półprzezroczysty panel na cały ekran).
   - Stwórz dwa puste kontenery na karty z komponentem **Horizontal Layout Group**:
     - **DealerCards** -> przeciągnij do **Dealer Card Container**.
     - **PlayerCards** -> przeciągnij do **Player Card Container**.
   - Przeciągnij odpowiednie teksty **TextMeshProUGUI**:
     - **Player Score Text** (np. *"Twoje punkty: 0"*)
     - **Dealer Score Text** (np. *"Krupier: 0"*)
     - **Player Caps Text** (np. *"Posiadane kapsle: 100"*)
     - **Current Bet Text** (np. *"Zakład: 10 kapsli"*)
     - **Dealer Dialogue Text** (dymek z komentarzem bota)
   - Przeciągnij przyciski:
     - **Deal Button** (Graj / Rozdaj)
     - **Hit Button** (Dobierz)
     - **Stand Button** (Pas)
     - **Double Button** (Podwój)
     - **Close Button** (Wyjście / X)
     - Przyciski stawek: `+5`, `+10`, `+25`, `+50`, `+100`, `Clear`, `All In`.

> **Wskazówka (Proceduralny Fallback):**  
> Jeśli zostawisz pole **Card Prefab** puste, system **sam wygeneruje proceduranie karty** ze wszystkimi tekstami i kolorami prosto w Twoim kontenerze!

---

## KROK 3: (Opcjonalnie) Własny Prefab Karty `CardUI`

Jeśli chcesz mieć ładny, własnoręcznie ostylowany szablon karty:
1. W Canvasie stwórz obiekt **UI > Image** (wymiary np. szerokość 90, wysokość 130).
2. Dodaj komponent **`CardUI`** (`Assets/Praktyki/BlackJack/CardUI.cs`).
3. Pod nim stwórz:
   - Obiekt `Front` (Image – białe tło z zaokrąglonymi rogami):
     - `TopLeftText` (TextMeshProUGUI w lewym górnym rogu)
     - `CenterSymbolText` (TextMeshProUGUI na środku z dużą czcionką)
     - `BottomRightText` (TextMeshProUGUI w prawym dolnym rogu)
   - Obiekt `Back` (Image – np. ciemnoczerwone tło z wzorkiem jako rewers karty).
4. Przeciągnij tak przygotowany obiekt do folderu `Assets/Praktyki/BlackJack/` jako Prefab, a następnie przypisz go do pola **Card Prefab** w `BlackJackUI`.

---

## Klawisze i Sterowanie w Minigrze:

- **Lewy Przycisk Myszy** na postać hazardzisty LUB klawisz **[E]** = Otwarcie minigry.
- **[Escape]** / **[E]** / Przycisk **X** = Wyjście z minigry i powrót do gry.
- **[Spacja]** / **[Enter]** = Rozdaj karty / Następna runda.
- **[H]** lub klawisz **[1]** = Dobierz kartę (Hit).
- **[S]** lub klawisz **[2]** = Pasuj (Stand).
- **[D]** lub klawisz **[3]** = Podwój stawkę (Double Down).
