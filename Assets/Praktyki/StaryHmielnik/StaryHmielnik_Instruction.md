# 🌾 Stary Hmielnik: Przeciwnik Dystansowy (Miotacz Szyszek Chmielu)

## 📌 Przegląd Implementacji
**Stary Hmielnik** to zaawansowany przeciwnik dystansowy. Zamiast szarżować na gracza, zachowuje dystans (kiting), obraca się w jego stronę i ciska spleśniałymi szyszkami chmielu na wyprzedzenie (predykcja ruchu gracza).

Trafienie szyszką nakłada **spowolnienie z lekką zieloną ramką na ekranie** oraz posiada **15% szansy na zatrucie gracza (10 HP/s przez 5s ze zmianą koloru paska życia na zielony)**.

---

## 📂 Pliki w projekcie (`Assets/Praktyki/StaryHmielnik/`):

1. **[`StaryHmielnikController.cs`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/StaryHmielnikController.cs)**:
   - Sztuczna inteligencja przeciwnika dystansowego (kiting, obracanie się, utrzymywanie dystansu).
   - **Rzut na wyprzedzenie** (`Lead Target Multiplier: 0.75`): Chmielnik analizuje kierunek i prędkość biegu gracza, rzucając szyszkę tam, gdzie gracz się znajdzie.
   - **Regulowany łuk rzutu** (`Upward Arc`): Umożliwia rzut lekkim łukiem balistycznym lub płaskim strzałem.
   - Wyrzut szyszki zsynchronizowany z klatką **0.45s** animacji `atack`.

2. **[`HopConeProjectile.cs`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/HopConeProjectile.cs)**:
   - Skrypt lecącej szyszki z grawitacją i koziołkowaniem (`360°/s`).
   - Prędkość wylotowa: `19 m/s`.
   - **Promień wybuchu zarodników (`Splash Radius: 1.6m`)**: Szyszka rozbija się o podłoże/ściany, raniąc i spowalniając gracza nawet przy bliskim uniku.
   - **15% szansy na zatrucie** (`Poison Chance: 0.15`, `10 HP/s`, `5s`).

3. **[`HopSlowDebuff.cs`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/HopSlowDebuff.cs)**:
   - Spowalnia gracza o 50% na 2.5s i automatycznie przywraca prędkość po wygaśnięciu.
   - Aktywuje subtelną zieloną winietę na ekranie na czas trwania spowolnienia.

4. **[`HopSlowVignetteUI.cs`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/HopSlowVignetteUI.cs)**:
   - Bardzo lekka, miękka zielona ramka wokół ekranu (delikatne pulsowanie, 100% czysty środek ekranu).
   - Samoczynnie podpina się pod Canvas w scenie i płynnie zanika po zakończeniu spowolnienia.

5. **[`HopPoisonDebuff.cs`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/HopPoisonDebuff.cs)**:
   - Zadaje **10 HP obrażeń co 1 sekundę przez 5 sekund** (łącznie 50 HP).
   - Przełącza kolor paska życia na **jadowity zielony** na czas trwania trucizny, a po wygaśnięciu przywraca pierwotny czerwony kolor.

6. **[`HealthBar.cs`](file:///g:/beergeon_v2/Assets/Scripts/UI/HealthBar.cs)**:
   - Zaktualizowany o obsługę stanu zatrucia (`SetPoisonState(bool)`).

7. **[`ChmielowaSzyszka.prefab`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/ChmielowaSzyszka.prefab)** & **[`ChmielowaSzyszka.png`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/ChmielowaSzyszka.png)**:
   - Skonfigurowany prefab pocisku wraz z czytelną grafiką szyszki chmielowej.

8. **[`StaryHmielnikVisuals.cs`](file:///g:/beergeon_v2/Assets/Praktyki/StaryHmielnik/StaryHmielnikVisuals.cs)**:
   - Presety wyglądu w Inspectorze (Spleśniały Brąz, Szara Pleśń, Zeschły Brąz, Zgnilizna).

---

## 🎨 Podmiana grafiki szyszki w przyszłości:
1. Przeciągnij swój plik PNG do `Assets/Praktyki/StaryHmielnik/` (Texture Type: *Sprite 2D and UI*).
2. Otwórz `ChmielowaSzyszka.prefab` i zaznacz dziecko `Visual_Szyszka`.
3. Przeciągnij nową grafikę do pola **Sprite** w komponencie *Sprite Renderer*.
