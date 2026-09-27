using System;
using System.Collections;
using UnityEngine;
using CurrencySystem;

namespace BlackJack
{
    public enum GameState
    {
        Betting,
        Dealing,
        PlayerTurn,
        DealerTurn,
        RoundOver
    }

    public enum RoundResult
    {
        None,
        PlayerBlackjack,
        PlayerWon,
        DealerBust,
        DealerWon,
        PlayerBust,
        Push // Remis
    }

    public class BlackJackGame : MonoBehaviour
    {
        public static BlackJackGame Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int minBet = 5;
        [SerializeField] private int maxBet = 1000;
        [SerializeField] private int defaultBet = 10;
        [SerializeField] private float dealCardDelay = 0.3f;
        [SerializeField] private float dealerHitDelay = 0.6f;

        public GameState CurrentState { get; private set; } = GameState.Betting;
        public int CurrentBet { get; private set; } = 0;
        public int LastBet { get; private set; } = 0;

        public BlackJackHand PlayerHand { get; } = new BlackJackHand();
        public BlackJackHand DealerHand { get; } = new BlackJackHand();

        private Deck deck;

        // Zdarzenia do powiadamiania UI
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnBetChanged;
        public event Action<BlackJackHand> OnPlayerCardAdded;
        public event Action<BlackJackHand, bool> OnDealerCardAdded;
        public event Action<BlackJackHand, bool> OnDealerCardRevealed;
        public event Action<RoundResult, int, string> OnRoundEnded; // result, payout, quote
        public event Action<string> OnDealerQuote;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            deck = new Deck(2); // Używamy 2 talii
        }

        private void Start()
        {
            SetInitialBet();
        }

        public void SetInitialBet()
        {
            int playerCaps = GetPlayerCaps();
            if (CurrentBet == 0)
            {
                CurrentBet = Mathf.Clamp(defaultBet, minBet, playerCaps > 0 ? playerCaps : defaultBet);
            }
            OnBetChanged?.Invoke(CurrentBet);
        }

        public int GetPlayerCaps()
        {
            return PlayerCurrency.Instance != null ? PlayerCurrency.Instance.CurrentCaps : 100;
        }

        #region Betting Controls

        public void AddBet(int amount)
        {
            if (CurrentState != GameState.Betting) return;

            int playerCaps = GetPlayerCaps();
            int newBet = CurrentBet + amount;

            if (newBet > playerCaps)
            {
                newBet = playerCaps;
            }

            if (newBet > maxBet)
            {
                newBet = maxBet;
            }

            CurrentBet = Mathf.Max(minBet, newBet);
            OnBetChanged?.Invoke(CurrentBet);
        }

        public void ClearBet()
        {
            if (CurrentState != GameState.Betting) return;

            CurrentBet = Mathf.Min(minBet, GetPlayerCaps());
            OnBetChanged?.Invoke(CurrentBet);
        }

        public void AllIn()
        {
            if (CurrentState != GameState.Betting) return;

            int playerCaps = GetPlayerCaps();
            CurrentBet = Mathf.Clamp(playerCaps, 0, maxBet);
            OnBetChanged?.Invoke(CurrentBet);
            TriggerDealerQuote("Oho, grubo! All-in?! Albo jesteś szalony, albo masz niesamowite szczęście!");
        }

        public void Rebet()
        {
            if (CurrentState != GameState.Betting) return;

            if (LastBet > 0)
            {
                int playerCaps = GetPlayerCaps();
                CurrentBet = Mathf.Clamp(LastBet, minBet, Mathf.Min(playerCaps, maxBet));
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        #endregion

        #region Game Flow

        public void StartDeal()
        {
            if (CurrentState != GameState.Betting) return;

            int playerCaps = GetPlayerCaps();
            if (CurrentBet <= 0 || playerCaps < CurrentBet)
            {
                TriggerDealerQuote("Brak kapsli, kolego! Nie grasz na krechę.");
                return;
            }

            // Pobieramy stawkę od gracza
            if (PlayerCurrency.Instance != null)
            {
                bool success = PlayerCurrency.Instance.TrySpendCaps(CurrentBet);
                if (!success)
                {
                    TriggerDealerQuote("Nie masz tylu kapsli!");
                    return;
                }
            }

            LastBet = CurrentBet;
            StartCoroutine(DealRoutine());
        }

        private IEnumerator DealRoutine()
        {
            SetState(GameState.Dealing);
            PlayerHand.Clear();
            DealerHand.Clear();

            // Losowy tekst bota na start
            TriggerRandomQuote(dealerStartQuotes);

            yield return new WaitForSecondsRealtime(dealCardDelay);

            // 1. Karta dla gracza
            Card pCard1 = deck.Draw();
            PlayerHand.AddCard(pCard1);
            OnPlayerCardAdded?.Invoke(PlayerHand);

            yield return new WaitForSecondsRealtime(dealCardDelay);

            // 2. Karta dla krupiera (odkryta)
            Card dCard1 = deck.Draw();
            DealerHand.AddCard(dCard1);
            OnDealerCardAdded?.Invoke(DealerHand, true);

            yield return new WaitForSecondsRealtime(dealCardDelay);

            // 3. Druga karta dla gracza
            Card pCard2 = deck.Draw();
            PlayerHand.AddCard(pCard2);
            OnPlayerCardAdded?.Invoke(PlayerHand);

            yield return new WaitForSecondsRealtime(dealCardDelay);

            // 4. Druga karta dla krupiera (zakryta!)
            Card dCard2 = deck.Draw();
            DealerHand.AddCard(dCard2);
            OnDealerCardAdded?.Invoke(DealerHand, false);

            yield return new WaitForSecondsRealtime(dealCardDelay);

            // Sprawdzenie natychmiastowego Blackjacka
            bool playerBJ = PlayerHand.IsBlackjack;
            bool dealerBJ = DealerHand.IsBlackjack;

            if (playerBJ || dealerBJ)
            {
                // Odkrywamy drugą kartę bota
                OnDealerCardRevealed?.Invoke(DealerHand, true);
                yield return new WaitForSecondsRealtime(0.5f);

                if (playerBJ && dealerBJ)
                {
                    EndRound(RoundResult.Push, CurrentBet, "Niewiarygodne... Oboje mamy Blackjacka! Remis!");
                }
                else if (playerBJ)
                {
                    // Wypłata 3:2 (stawka + 1.5x)
                    int bonus = Mathf.RoundToInt(CurrentBet * 1.5f);
                    int payout = CurrentBet + bonus;
                    EndRound(RoundResult.PlayerBlackjack, payout, "CO?! Naturalne Oczko?! No toś mnie urządził...");
                }
                else
                {
                    EndRound(RoundResult.DealerWon, 0, "Oczko dla kasyna! Szkoła mistrzów, kapselki są moje!");
                }
                yield break;
            }

            SetState(GameState.PlayerTurn);
        }

        public void Hit()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            Card card = deck.Draw();
            PlayerHand.AddCard(card);
            OnPlayerCardAdded?.Invoke(PlayerHand);

            int score = PlayerHand.CalculateScore();

            if (score > 21)
            {
                // Fura (Bust)
                TriggerRandomQuote(dealerBustPlayerQuotes);
                EndRound(RoundResult.PlayerBust, 0, GetRandomQuote(dealerBustPlayerQuotes));
            }
            else if (score == 21)
            {
                // Równe 21 -> automatyczny pas
                Stand();
            }
            else
            {
                TriggerRandomQuote(dealerPlayerHitQuotes);
            }
        }

        public void Stand()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            StartCoroutine(DealerTurnRoutine());
        }

        public void DoubleDown()
        {
            if (CurrentState != GameState.PlayerTurn) return;
            if (PlayerHand.Cards.Count != 2) return;

            int playerCaps = GetPlayerCaps();
            if (playerCaps < CurrentBet)
            {
                TriggerDealerQuote("Nie masz dość kapsli na podwojenie!");
                return;
            }

            if (PlayerCurrency.Instance != null)
            {
                PlayerCurrency.Instance.TrySpendCaps(CurrentBet);
            }

            CurrentBet *= 2;
            OnBetChanged?.Invoke(CurrentBet);
            TriggerDealerQuote("Podwajasz stawki?! Lubię ludzi z fantazją! Masz jedną kartę!");

            // Dokładnie jedna karta i natychmiastowy Stand
            Card card = deck.Draw();
            PlayerHand.AddCard(card);
            OnPlayerCardAdded?.Invoke(PlayerHand);

            int score = PlayerHand.CalculateScore();
            if (score > 21)
            {
                EndRound(RoundResult.PlayerBust, 0, "Podwoiłeś i fura! Dzięki za podwójną wypłatę, mistrzu!");
            }
            else
            {
                StartCoroutine(DealerTurnRoutine());
            }
        }

        private IEnumerator DealerTurnRoutine()
        {
            SetState(GameState.DealerTurn);

            // Odkrycie drugiej karty krupiera
            OnDealerCardRevealed?.Invoke(DealerHand, true);
            yield return new WaitForSecondsRealtime(dealerHitDelay);

            int playerScore = PlayerHand.CalculateScore();

            // Hazardzista gra inteligentnie:
            // Jeśli już ma WIĘCEJ punktów niż gracz (i nie ma fury), natychmiast pasuje i zgarnia wygraną!
            // Dobiera tylko wtedy, gdy przegrywa lub przy remisie poniżej 17 punktów.
            while (ShouldDealerHit(DealerHand.CalculateScore(), playerScore))
            {
                Card card = deck.Draw();
                DealerHand.AddCard(card);
                OnDealerCardAdded?.Invoke(DealerHand, true);
                yield return new WaitForSecondsRealtime(dealerHitDelay);
            }

            // Rozstrzygnięcie wyniku
            EvaluateOutcome();
        }

        private bool ShouldDealerHit(int dealerScore, int playerScore)
        {
            // Jeśli krupier ma 21 lub furę, nie dobiera
            if (dealerScore >= 21) return false;

            // Jeśli krupier już ma WIĘCEJ punktów niż gracz, nie ryzykuje fury i pasuje!
            if (dealerScore > playerScore) return false;

            // Jeśli krupier ma MNIEJ punktów niż gracz, MUSI dobierać, bo inaczej przegra
            if (dealerScore < playerScore) return true;

            // W przypadku remisu (dealerScore == playerScore):
            // Jeśli ma 17 lub więcej, zostawia remis. Poniżej 17 ryzykuje i dobiera.
            return dealerScore < 17;
        }

        private void EvaluateOutcome()
        {
            int playerScore = PlayerHand.CalculateScore();
            int dealerScore = DealerHand.CalculateScore();

            if (dealerScore > 21)
            {
                // Krupier przekroczył 21 (Bust)
                int payout = CurrentBet * 2;
                EndRound(RoundResult.DealerBust, payout, "Niech to szlag... Przelicytowałem! Twoja wygrana.");
            }
            else if (playerScore > dealerScore)
            {
                // Gracz ma więcej punktów
                int payout = CurrentBet * 2;
                EndRound(RoundResult.PlayerWon, payout, GetRandomQuote(dealerPlayerWonQuotes));
            }
            else if (playerScore < dealerScore)
            {
                // Krupier ma więcej punktów
                EndRound(RoundResult.DealerWon, 0, GetRandomQuote(dealerWonQuotes));
            }
            else
            {
                // Remis (Push) - zwrot postawionych kapsli
                EndRound(RoundResult.Push, CurrentBet, "Remis! Kapsle wracają do właściciela.");
            }
        }

        private void EndRound(RoundResult result, int payout, string quote)
        {
            SetState(GameState.RoundOver);

            if (payout > 0 && PlayerCurrency.Instance != null)
            {
                PlayerCurrency.Instance.AddCaps(payout);
            }

            OnRoundEnded?.Invoke(result, payout, quote);
        }

        public void PrepareNewRound()
        {
            if (CurrentState != GameState.RoundOver) return;

            PlayerHand.Clear();
            DealerHand.Clear();

            int playerCaps = GetPlayerCaps();
            if (CurrentBet > playerCaps)
            {
                CurrentBet = Mathf.Max(0, playerCaps);
            }
            if (CurrentBet < minBet && playerCaps >= minBet)
            {
                CurrentBet = minBet;
            }

            OnBetChanged?.Invoke(CurrentBet);
            SetState(GameState.Betting);
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        #endregion

        #region Quotes / Teksty Bota Hazardzisty

        private readonly string[] dealerStartQuotes = new string[]
        {
            "Karty rozdane! Zobaczmy co tam masz w zanadrzu.",
            "Stawiasz kapsle na stół, a ja biorę się do roboty!",
            "Oby los ci sprzyjał... ale nie za bardzo, hehe.",
            "Poczuj ten dreszczyk emocji!"
        };

        private readonly string[] dealerPlayerHitQuotes = new string[]
        {
            "Dobierasz? Odważnie...",
            "Ryzykujesz kolego, stół bywa bezlitosny!",
            "Kolejna karta na twoje konto.",
            "Ciekawe czy to zbliży cię do celu, czy do przepaści."
        };

        private readonly string[] dealerBustPlayerQuotes = new string[]
        {
            "FURA! Chciwość zgubiła kolejnego śmiałka!",
            "Ponad 21! Kapselki trafiają prosto do mojej kieszeni.",
            "Przeliczyłeś się, przyjacielu! Dzięki za datek!",
            "Za dużo! Następnym razem spasuj wcześniej."
        };

        private readonly string[] dealerPlayerWonQuotes = new string[]
        {
            "Ehh, fartowny rzut... Masz te kapsle.",
            "Niezła ręka. Następnym razem nie pójdzie ci tak łatwo!",
            "Czysty przypadek! Odbiję to sobie w następnej partii.",
            "Wygrałeś tę rundę, ale kasyno zawsze na końcu wygrywa."
        };

        private readonly string[] dealerWonQuotes = new string[]
        {
            "Wygrałem! Szczęście sprzyja zawodowcom!",
            "Moja ręka jest lepsza! Kapsle zostają u mnie.",
            "Dzięki za kapsle, postawię za nie kolejkę w barze!",
            "Z krupierem się nie wygrywa, młodzieńcze."
        };

        private void TriggerRandomQuote(string[] quotes)
        {
            if (quotes == null || quotes.Length == 0) return;
            string q = quotes[UnityEngine.Random.Range(0, quotes.Length)];
            TriggerDealerQuote(q);
        }

        private string GetRandomQuote(string[] quotes)
        {
            if (quotes == null || quotes.Length == 0) return "";
            return quotes[UnityEngine.Random.Range(0, quotes.Length)];
        }

        public void TriggerDealerQuote(string quote)
        {
            OnDealerQuote?.Invoke(quote);
        }

        #endregion
    }
}
