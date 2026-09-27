# ⚡ Poradnik: System Staminy, Sprintu i Uniku (Dash)

W projekcie dodano obsługę zwinnego uniku (**Dodge Dash**) pod klawiszem **Lewy Ctrl**, który logicznie integruje się z paskiem staminy oraz walką.

---

## 🎮 Sterowanie i Działanie Dasha

- **Klawisz aktywacji**: **Left Ctrl** (lub Prawy Ctrl).
- **Kierunek uniku**:
  - Jeśli gracz porusza się za pomocą **WASD**, dash wykonuje się w kierunku wciskanego ruchu (np. wciśnięte `A` = unik w lewo, `D` = w prawo, `S` = odskok w tył, `W` = zryw w przód).
  - Jeśli gracz stoi w miejscu, dash wykonuje się w kierunku patrzenia (`transform.forward`).
- **Zużycie Staminy**: **`8 punktów staminy`** na jeden unik (przy bazowej staminie 30 pkt pozwala to na 3 zwinne uniki pod rząd). Jeśli gracz ma mniej niż 8 pkt staminy, dash nie odpali się, dopóki pasek się nie zregeneruje.
- **Czas trwania i prędkość**:
  - Prędkość impulsu: `13.5 m/s`
  - Czas trwania: `0.18 sekundy` (~2.4 metra dynamicznego zrywu).
  - Cooldown: `0.5 sekundy`.
- **Ramki Nietykalności (I-Frames)**:
  - Podczas trwania dasha gracz ma aktywną flagę `HasIFrames`, dzięki czemu ciosy wrogów nie zadają obrażeń w ułamku sekundy uniku (perfekcyjny unik).

---

## 📂 Zmodyfikowane Pliki

1. **[`PlayerController.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerController.cs)**:
   - Dodano sekcję `[Header("Dash / Dodge Settings")]` z parametrami: `dashSpeed`, `dashDuration`, `dashCooldown`, `dashStaminaCost`, `giveIFramesDuringDash`.
   - Obsługa wejścia `L-Ctrl` i odliczania cooldownu.
   - Metoda `TryPerformDash(Vector3 currentMoveDir, bool isMoving)`.
2. **[`PlayerHealth.cs`](file:///g:/beergeon_v2/Assets/Scripts/Player/PlayerHealth.cs)**:
   - W `TakeDamage(int damage)` dodano sprawdzanie `playerController.HasIFrames`.
   - Zapewnia to omijanie obrażeń przy zręcznym zrobieniu uniku pod cios potwora.
