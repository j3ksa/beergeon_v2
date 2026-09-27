# 💥 Poradnik Implementacji: Latające Cyferki Obrażeń (Damage Numbers)

Kompletny, soczysty i wydajny system wyświetlania unoszących się liczb zadanych obrażeń (**Floating Damage Popups**) został w pełni zaimplementowany i zintegrowany z mechaniką walki w projekcie.

Folder główny: `Assets/Praktyki/Cyferkiobrazen/`  
Czcionka: `Ibarra Real Nova-VariableFont_wght SDF`

---

## 📂 Zawartość folderu `Assets/Praktyki/Cyferkiobrazen/`

| Plik | Opis |
| :--- | :--- |
| **[`DamagePopup.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Cyferkiobrazen/DamagePopup.cs)** | Komponent pojedynczej cyferki. Odpowiada za: soczysty **Punch Scale** (szybkie powiększenie i miękkie opadnięcie), ruch po losowym łuku, płynny fade-out alfy, **Billboard 3D** (zawsze twarzą do kamery gracza) oraz zwrot do puli. |
| **[`DamagePopupManager.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Cyferkiobrazen/DamagePopupManager.cs)** | Globalny menedżer i pula obiektów (**Object Pooling**). Eliminuje spadki FPS i alokacje pamięci GC. Udostępnia proste API `DamagePopupManager.Show(...)` i automatyczny fallback. |
| **[`DamageType.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Cyferkiobrazen/DamageType.cs)** | Enum rodzajów obrażeń: `Normal`, `Critical`, `Poison`, `Bleed`, `Heal`. |
| **[`DamagePopupContext.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Cyferkiobrazen/DamagePopupContext.cs)** | Lekki kontekst statyczny pozwalający efektom okresowym (DoT) lub atakom krytycznym łatwo ustawić styl cyferki przed wywołaniem `TakeDamage`. |
| **[`DamagePopup.prefab`](file:///g:/beergeon_v2/Assets/Praktyki/Cyferkiobrazen/DamagePopup.prefab)** | Gotowy prefab z podpiętą czcionką **Ibarra Real Nova SDF**, skonfigurowanym TextMeshPro 3D i parametrami animacji. |
| **[`DamagePopupEditorUtils.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Cyferkiobrazen/Editor/DamagePopupEditorUtils.cs)** | Narzędzie edytora pod górnym paskiem Unity (`Tools > Cyferki Obrażeń`), pozwalające jednym kliknięciem odświeżyć prefab lub dodać menedżer do dowolnej sceny. |

---

## 🎨 Wizualne i Soczyste Urozmaicenia (Game Feel / Juice)

1. **Soczysty Punch-Scale & Arc**:
   - Cyfra wyskakuje z dynamicznym, dyskretnym „punch scale” (delikatne powiększenie do 115-120%), po czym unosi się po losowym delikatnym łuku (rozrzut lewo/prawo), co zapobiega zlewaniu się cyferek przy szybkich seriach uderzeń.
2. **Krytyki (Critical Hits)**:
   - Duże ciosy (domyślnie obrażenia $\ge 25$ lub wywołane ze statusem krytyka, np. cios w plecy ze sztyletu):
     - Zbalansowany rozmiar czcionki (3.2 zamiast zwykłego 2.4)
     - Ognisty, pomarańczowo-czerwony kolor (`#FF4014`)
     - Czysta, czytelna cyfra bez wykrzykników
3. **Zwykłe Trafienia**:
   - Kompaktowy rozmiar (2.4), ciepły złocisto-żółty kolor (`#FFF299`), wyraźny i niezasłaniający pola widzenia.
4. **Obrażenia Okresowe (DoT)**:
   - **Trucizna (Zatruty Miecz)**: mniejsza jadowicie zielona cyferka (`#33FF5A`).
   - **Krwawienie (Sztylet / Sierp)**: mniejsza głęboko karmazynowa cyferka (`#E01426`).
5. **Billboard 3D**:
   - W każdej klatce (`LateUpdate`) rotacja obiektu jest wyrównana z rotacją `Camera.main`, dzięki czemu pod każdym kątem i podczas obracania się gracza cyfry są idealnie czytelne i skierowane do ekranu.
6. **Object Pooling**:
   - Brak ciągłego `Instantiate` i `Destroy`. Po zakończeniu animacji cyfra jest deaktywowana i wraca do kolejki `pool`, gotowa do natychmiastowego ponownego użycia.

---

## ⚙️ Wprowadzone Integracje w Projekcie

1. **Wszyscy Wrogowie ([`EnemyHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Enemy/EnemyHealth.cs))**:
   - W metodzie `TakeDamage(int damage)` dodano automatyczne wywołanie:
     ```csharp
     DamageNumbers.DamagePopupManager.Show(transform.position + Vector3.up * 1.5f, damage);
     ```
   - Każdy przeciwnik (Hmielnik, Stary Hmielnik i inni) natychmiast wyrzuca cyferki przy trafieniu!
2. **Manekin Treningowy ([`TrainingDummy.cs`](file:///g:/beergeon_v2/Assets/Scripts/Systems/TrainingDummy.cs))**:
   - Dodano wywołanie w `TakeDamage()`, więc na manekinie treningowym cyferki działają od razu.
3. **Zatruty Miecz ([`PoisonStatusEffect.cs`](file:///g:/beergeon_v2/Assets/Praktyki/ZatrutyMiecz/PoisonStatusEffect.cs))**:
   - Przed każdym tikiem trucizny ustawia kontekst `DamageType.Poison`, dzięki czemu cyferki DoT są zielone.
4. **Sztylet ([`SztyletItem.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/SztyletItem.cs) & [`BleedStatusEffect.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sztylet/BleedStatusEffect.cs))**:
   - Krytyki sztyletu automatycznie oznaczają trafienie jako `DamageType.Critical`.
   - Tiki krwawienia oznaczają trafienie jako `DamageType.Bleed` (karmazyn).
5. **Sierp ([`BleedEffect.cs`](file:///g:/beergeon_v2/Assets/Praktyki/Sierp/BleedEffect.cs))**:
   - Tiki krwawienia oznaczone jako `DamageType.Bleed`.

---

## 🚀 Jak Przetestować w Unity Editorze

1. **Opcjonalnie (jeśli chcesz mieć menadżer w hierarchii sceny)**:
   - W górnym menu Unity kliknij:
     `Tools > Cyferki Obrażeń > 2. Dodaj Menadżer Cyferek do Sceny`.
   - *(Nawet bez tego menadżer utworzy się automatycznie w locie przy pierwszym uderzeniu wroga!)*.
2. **Wciśnij Play**:
   - Uderz dowolnego przeciwnika lub manekina treningowego (dłonią, mieczem, sztyletem, kuszą, włócznią itd.).
   - Nad wrogiem wystrzelą złote cyferki z soczystą animacją powiększenia i opadania.
   - Trafienie krytyczne lub cios $\ge 25$ DMG pokaże wielką czerwoną liczbę z wykrzyknikiem.
   - Nałożenie trucizny lub krwawienia pokaże zielone i bordowe cyferki przy każdym tiku!
