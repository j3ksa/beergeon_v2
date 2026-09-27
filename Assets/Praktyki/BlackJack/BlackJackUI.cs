using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CurrencySystem;

namespace BlackJack
{
    public class BlackJackUI : MonoBehaviour
    {
        private static BlackJackUI _instance;
        public static BlackJackUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<BlackJackUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [Header("Main Panels")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private Button closeButton;

        [Header("Game Engine Reference")]
        [SerializeField] private BlackJackGame game;

        [Header("Card Containers & Prefab")]
        [SerializeField] private Transform playerCardContainer;
        [SerializeField] private Transform dealerCardContainer;
        [SerializeField] private GameObject cardPrefab;

        [Header("Score & Status Texts")]
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI dealerScoreText;
        [SerializeField] private TextMeshProUGUI playerCapsText;
        [SerializeField] private TextMeshProUGUI currentBetText;
        [SerializeField] private TextMeshProUGUI potText;

        [Header("Hazardzista Dialogue")]
        [SerializeField] private TextMeshProUGUI dealerDialogueText;
        [SerializeField] private string dealerName = "Hazardzista";

        [Header("Result Banner")]
        [SerializeField] private GameObject resultBanner;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultPayoutText;

        [Header("Control Groups")]
        [SerializeField] private GameObject bettingControlsGroup;
        [SerializeField] private GameObject gameplayControlsGroup;
        [SerializeField] private GameObject endRoundControlsGroup;

        [Header("Buttons - Betting")]
        [SerializeField] private Button dealButton;
        [SerializeField] private Button clearBetButton;
        [SerializeField] private Button allInButton;
        [SerializeField] private Button betAdd5Button;
        [SerializeField] private Button betAdd10Button;
        [SerializeField] private Button betAdd25Button;
        [SerializeField] private Button betAdd50Button;
        [SerializeField] private Button betAdd100Button;

        [Header("Buttons - Gameplay")]
        [SerializeField] private Button hitButton;
        [SerializeField] private Button standButton;
        [SerializeField] private Button doubleButton;

        [Header("Buttons - End Round")]
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private Button rebetButton;

        [Header("HUD Management")]
        [SerializeField] private List<GameObject> hudElementsToHide = new List<GameObject>();

        private readonly List<CardUI> playerCardsUI = new List<CardUI>();
        private readonly List<CardUI> dealerCardsUI = new List<CardUI>();
        private List<GameObject> previouslyActiveHUDElements = new List<GameObject>();

        private bool isOpen = false;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (game == null)
            {
                game = GetComponent<BlackJackGame>();
                if (game == null) game = gameObject.AddComponent<BlackJackGame>();
            }
        }

        private void Start()
        {
            RegisterButtonCallbacks();
            SubscribeToGameEvents();
            AutoFindHUDElements();

            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }

        private void RegisterButtonCallbacks()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseGame);

            // Betting
            if (dealButton != null) dealButton.onClick.AddListener(() => game.StartDeal());
            if (clearBetButton != null) clearBetButton.onClick.AddListener(() => game.ClearBet());
            if (allInButton != null) allInButton.onClick.AddListener(() => game.AllIn());

            if (betAdd5Button != null) betAdd5Button.onClick.AddListener(() => game.AddBet(5));
            if (betAdd10Button != null) betAdd10Button.onClick.AddListener(() => game.AddBet(10));
            if (betAdd25Button != null) betAdd25Button.onClick.AddListener(() => game.AddBet(25));
            if (betAdd50Button != null) betAdd50Button.onClick.AddListener(() => game.AddBet(50));
            if (betAdd100Button != null) betAdd100Button.onClick.AddListener(() => game.AddBet(100));

            // Gameplay
            if (hitButton != null) hitButton.onClick.AddListener(() => game.Hit());
            if (standButton != null) standButton.onClick.AddListener(() => game.Stand());
            if (doubleButton != null) doubleButton.onClick.AddListener(() => game.DoubleDown());

            // End Round
            if (nextRoundButton != null) nextRoundButton.onClick.AddListener(() => game.PrepareNewRound());
            if (rebetButton != null) rebetButton.onClick.AddListener(() =>
            {
                game.Rebet();
                game.StartDeal();
            });
        }

        private void SubscribeToGameEvents()
        {
            if (game == null) return;

            game.OnStateChanged += HandleStateChanged;
            game.OnBetChanged += HandleBetChanged;
            game.OnPlayerCardAdded += HandlePlayerCardAdded;
            game.OnDealerCardAdded += HandleDealerCardAdded;
            game.OnDealerCardRevealed += HandleDealerCardRevealed;
            game.OnRoundEnded += HandleRoundEnded;
            game.OnDealerQuote += HandleDealerQuote;
        }

        private void UnsubscribeFromGameEvents()
        {
            if (game == null) return;

            game.OnStateChanged -= HandleStateChanged;
            game.OnBetChanged -= HandleBetChanged;
            game.OnPlayerCardAdded -= HandlePlayerCardAdded;
            game.OnDealerCardAdded -= HandleDealerCardAdded;
            game.OnDealerCardRevealed -= HandleDealerCardRevealed;
            game.OnRoundEnded -= HandleRoundEnded;
            game.OnDealerQuote -= HandleDealerQuote;
        }

        private void Update()
        {
            if (!isOpen) return;

            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            bool closePressed = false;
            bool hitPressed = false;
            bool standPressed = false;
            bool doublePressed = false;
            bool confirmPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.escapeKey.wasPressedThisFrame) closePressed = true;
                if (kb.hKey.wasPressedThisFrame || kb.digit1Key.wasPressedThisFrame) hitPressed = true;
                if (kb.sKey.wasPressedThisFrame || kb.digit2Key.wasPressedThisFrame) standPressed = true;
                if (kb.dKey.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame) doublePressed = true;
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) confirmPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape)) closePressed = true;
            if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Alpha1)) hitPressed = true;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Alpha2)) standPressed = true;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Alpha3)) doublePressed = true;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) confirmPressed = true;
#endif

            if (closePressed)
            {
                CloseGame();
                return;
            }

            if (game.CurrentState == GameState.PlayerTurn)
            {
                if (hitPressed) game.Hit();
                else if (standPressed) game.Stand();
                else if (doublePressed) game.DoubleDown();
            }
            else if (game.CurrentState == GameState.Betting && confirmPressed)
            {
                game.StartDeal();
            }
            else if (game.CurrentState == GameState.RoundOver && confirmPressed)
            {
                game.PrepareNewRound();
            }
        }

        #region Open / Close Game Window

        public void OpenGame()
        {
            isOpen = true;

            Transform current = windowRoot != null ? windowRoot.transform : transform;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                if (current.GetComponent<Canvas>() != null) break;
                current = current.parent;
            }

            if (windowRoot != null) windowRoot.SetActive(true);
            else gameObject.SetActive(true);

            HideActiveHUD();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;

            game.SetInitialBet();
            UpdateCapsDisplay();
            game.TriggerDealerQuote("SIEMASZ! MASZ OCHOTĘ NA MAŁE PARTYJKI OCZKA? KAPSLE NA STÓŁ!");
        }

        public void CloseGame()
        {
            isOpen = false;
            UIOverlayManager.NotifyOverlayClosed();

            if (windowRoot != null) windowRoot.SetActive(false);
            else gameObject.SetActive(false);

            RestoreHUD();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }

        public void ToggleGame()
        {
            if (isOpen) CloseGame();
            else OpenGame();
        }

        #endregion

        #region Event Handlers

        private void HandleStateChanged(GameState state)
        {
            if (bettingControlsGroup != null) bettingControlsGroup.SetActive(state == GameState.Betting);
            if (gameplayControlsGroup != null) gameplayControlsGroup.SetActive(state == GameState.PlayerTurn);
            if (endRoundControlsGroup != null) endRoundControlsGroup.SetActive(state == GameState.RoundOver);

            if (resultBanner != null && state != GameState.RoundOver)
            {
                resultBanner.SetActive(false);
            }

            if (state == GameState.Betting)
            {
                ClearCardObjects();
                UpdateScoreTexts(0, 0);
            }

            // Możliwość podwojenia tylko przy pierwszej parze kart
            if (doubleButton != null)
            {
                bool canDouble = state == GameState.PlayerTurn &&
                                 game.PlayerHand.Cards.Count == 2 &&
                                 game.GetPlayerCaps() >= game.CurrentBet;
                doubleButton.interactable = canDouble;
            }

            UpdateCapsDisplay();
        }

        private void HandleBetChanged(int bet)
        {
            if (currentBetText != null)
            {
                currentBetText.text = $"ZAKŁAD: {bet} KAPSLI";
            }

            if (potText != null)
            {
                potText.text = $"PULA: {bet * 2} KAPSLI";
            }

            if (dealButton != null)
            {
                dealButton.interactable = (bet > 0 && bet <= game.GetPlayerCaps());
            }

            UpdateCapsDisplay();
        }

        private void HandlePlayerCardAdded(BlackJackHand hand)
        {
            int index = hand.Cards.Count - 1;
            Card newCard = hand.Cards[index];

            CardUI cUI = SpawnCardUI(newCard, playerCardContainer, true);
            playerCardsUI.Add(cUI);

            int score = hand.CalculateScore();
            if (playerScoreText != null)
            {
                playerScoreText.text = $"TWOJE PUNKTY: {score}";
            }
        }

        private void HandleDealerCardAdded(BlackJackHand hand, bool faceUp)
        {
            int index = hand.Cards.Count - 1;
            Card newCard = hand.Cards[index];

            CardUI cUI = SpawnCardUI(newCard, dealerCardContainer, faceUp);
            dealerCardsUI.Add(cUI);

            // Jeśli zakryta, pokazujemy tylko punkty pierwszej karty
            int score = hand.CalculateScore(!faceUp);
            if (dealerScoreText != null)
            {
                dealerScoreText.text = faceUp ? $"KRUPIER: {score}" : $"KRUPIER: {score} + ?";
            }
        }

        private void HandleDealerCardRevealed(BlackJackHand hand, bool revealAll)
        {
            if (dealerCardsUI.Count > 1)
            {
                dealerCardsUI[1].Reveal(true);
            }

            int score = hand.CalculateScore(false);
            if (dealerScoreText != null)
            {
                dealerScoreText.text = $"KRUPIER: {score}";
            }
        }

        private void HandleRoundEnded(RoundResult result, int payout, string quote)
        {
            UpdateCapsDisplay();

            if (resultBanner != null)
            {
                resultBanner.SetActive(true);
            }

            string title = "";
            string payoutStr = "";
            Color bannerColor = Color.white;

            switch (result)
            {
                case RoundResult.PlayerBlackjack:
                    title = "BLACKJACK! (21)";
                    payoutStr = $"+{payout} KAPSLI! (WYPŁATA 3:2)";
                    bannerColor = new Color(1f, 0.84f, 0f); // Złoty
                    break;
                case RoundResult.PlayerWon:
                    title = "WYGRANA!";
                    payoutStr = $"+{payout} KAPSLI!";
                    bannerColor = new Color(0.2f, 0.85f, 0.3f); // Zielony
                    break;
                case RoundResult.DealerBust:
                    title = "KRUPIER PRZELICYTOWAŁ!";
                    payoutStr = $"+{payout} KAPSLI!";
                    bannerColor = new Color(0.2f, 0.85f, 0.3f);
                    break;
                case RoundResult.Push:
                    title = "REMIS!";
                    payoutStr = $"ZWROT STAWKI ({payout} KAPSLI)";
                    bannerColor = new Color(0.3f, 0.7f, 1f); // Niebieski
                    break;
                case RoundResult.PlayerBust:
                    title = "FURA (PRZEGRANA)";
                    payoutStr = $"-{game.CurrentBet} KAPSLI";
                    bannerColor = new Color(0.9f, 0.25f, 0.25f); // Czerwony
                    break;
                case RoundResult.DealerWon:
                    title = "PRZEGRANA";
                    payoutStr = $"-{game.CurrentBet} KAPSLI";
                    bannerColor = new Color(0.9f, 0.25f, 0.25f);
                    break;
            }

            if (resultTitleText != null)
            {
                resultTitleText.text = title;
                resultTitleText.color = bannerColor;
            }

            if (resultPayoutText != null)
            {
                resultPayoutText.text = payoutStr;
            }

            if (!string.IsNullOrEmpty(quote))
            {
                HandleDealerQuote(quote);
            }
        }

        private void HandleDealerQuote(string quote)
        {
            if (dealerDialogueText != null)
            {
                dealerDialogueText.text = $"<b>{dealerName.ToUpper()}:</b> \"{quote.ToUpper()}\"";
            }
        }

        #endregion

        #region Card Spawning & Management

        private CardUI SpawnCardUI(Card card, Transform container, bool faceUp)
        {
            GameObject obj;
            if (cardPrefab != null)
            {
                obj = Instantiate(cardPrefab, container != null ? container : transform);
            }
            else
            {
                obj = CreateFallbackCardObject(container);
            }

            CardUI cardUI = obj.GetComponent<CardUI>();
            if (cardUI == null)
            {
                cardUI = obj.AddComponent<CardUI>();
            }

            cardUI.SetCard(card, faceUp);
            return cardUI;
        }

        private void ClearCardObjects()
        {
            foreach (var c in playerCardsUI)
            {
                if (c != null) Destroy(c.gameObject);
            }
            playerCardsUI.Clear();

            foreach (var c in dealerCardsUI)
            {
                if (c != null) Destroy(c.gameObject);
            }
            dealerCardsUI.Clear();
        }

        private void UpdateScoreTexts(int pScore, int dScore)
        {
            if (playerScoreText != null) playerScoreText.text = $"TWOJE PUNKTY: {pScore}";
            if (dealerScoreText != null) dealerScoreText.text = $"KRUPIER: {dScore}";
        }

        private void UpdateCapsDisplay()
        {
            int caps = game != null ? game.GetPlayerCaps() : 0;
            if (playerCapsText != null)
            {
                playerCapsText.text = $"POSIADANE KAPSLE: {caps}";
            }
        }

        /// <summary>
        /// Tworzy prosty obiekt karty w Canvasie, jeśli gracz nie przypisał jeszcze prefabu CardUI.
        /// </summary>
        private GameObject CreateFallbackCardObject(Transform parent)
        {
            GameObject cardObj = new GameObject("ProceduralCard", typeof(RectTransform));
            cardObj.transform.SetParent(parent != null ? parent : transform, false);

            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 130);

            // Front
            GameObject front = new GameObject("Front", typeof(RectTransform), typeof(Image));
            front.transform.SetParent(cardObj.transform, false);
            RectTransform frontRT = front.GetComponent<RectTransform>();
            frontRT.anchorMin = Vector2.zero;
            frontRT.anchorMax = Vector1;
            frontRT.sizeDelta = Vector2.zero;

            Image frontBg = front.GetComponent<Image>();
            frontBg.color = new Color(0.95f, 0.92f, 0.84f, 1f); // Stary pergamin / kość słoniowa

            TMP_FontAsset activeFont = playerScoreText != null ? playerScoreText.font : null;

            // TopLeftText
            GameObject tlObj = new GameObject("TopLeftText", typeof(RectTransform), typeof(TextMeshProUGUI));
            tlObj.transform.SetParent(front.transform, false);
            RectTransform tlRT = tlObj.GetComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0, 1);
            tlRT.anchorMax = new Vector2(0, 1);
            tlRT.pivot = new Vector2(0, 1);
            tlRT.anchoredPosition = new Vector2(6, -6);
            tlRT.sizeDelta = new Vector2(30, 45);
            TextMeshProUGUI tlTMP = tlObj.GetComponent<TextMeshProUGUI>();
            if (activeFont != null) tlTMP.font = activeFont;
            tlTMP.fontSize = 20;
            tlTMP.fontStyle = FontStyles.Bold;
            tlTMP.alignment = TextAlignmentOptions.TopLeft;

            // CenterSymbolText
            GameObject centerObj = new GameObject("CenterSymbolText", typeof(RectTransform), typeof(TextMeshProUGUI));
            centerObj.transform.SetParent(front.transform, false);
            RectTransform cRT = centerObj.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.5f, 0.5f);
            cRT.anchorMax = new Vector2(0.5f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.anchoredPosition = Vector2.zero;
            cRT.sizeDelta = new Vector2(50, 50);
            TextMeshProUGUI cTMP = centerObj.GetComponent<TextMeshProUGUI>();
            if (activeFont != null) cTMP.font = activeFont;
            cTMP.fontSize = 36;
            cTMP.alignment = TextAlignmentOptions.Center;

            // BottomRightText
            GameObject brObj = new GameObject("BottomRightText", typeof(RectTransform), typeof(TextMeshProUGUI));
            brObj.transform.SetParent(front.transform, false);
            RectTransform brRT = brObj.GetComponent<RectTransform>();
            brRT.anchorMin = new Vector2(1, 0);
            brRT.anchorMax = new Vector2(1, 0);
            brRT.pivot = new Vector2(1, 0);
            brRT.anchoredPosition = new Vector2(-6, 6);
            brRT.sizeDelta = new Vector2(30, 45);
            TextMeshProUGUI brTMP = brObj.GetComponent<TextMeshProUGUI>();
            if (activeFont != null) brTMP.font = activeFont;
            brTMP.fontSize = 20;
            brTMP.fontStyle = FontStyles.Bold;
            brTMP.alignment = TextAlignmentOptions.BottomRight;

            // Back (Karmazynowy rewers tawerniany)
            GameObject back = new GameObject("Back", typeof(RectTransform), typeof(Image));
            back.transform.SetParent(cardObj.transform, false);
            RectTransform backRT = back.GetComponent<RectTransform>();
            backRT.anchorMin = Vector2.zero;
            backRT.anchorMax = Vector1;
            backRT.sizeDelta = Vector2.zero;
            Image backBg = back.GetComponent<Image>();
            backBg.color = new Color(0.48f, 0.12f, 0.12f, 1f);

            return cardObj;
        }

        private static readonly Vector2 Vector1 = new Vector2(1, 1);

        #endregion

        #region HUD Management

        private void AutoFindHUDElements()
        {
            if (hudElementsToHide == null) hudElementsToHide = new List<GameObject>();

            string[] targetHUDNames = new string[] { "HealthBar", "StaminaBar", "XPBar", "Slots", "UIDesign", "PotionEffects", "MonkeyEffect" };
            foreach (string hName in targetHUDNames)
            {
                GameObject found = GameObject.Find(hName);
                if (found != null && !hudElementsToHide.Contains(found))
                {
                    hudElementsToHide.Add(found);
                }
            }
        }

        private void HideActiveHUD()
        {
            previouslyActiveHUDElements.Clear();
            if (hudElementsToHide == null) return;

            foreach (var hud in hudElementsToHide)
            {
                if (hud != null && hud.activeSelf)
                {
                    previouslyActiveHUDElements.Add(hud);
                    hud.SetActive(false);
                }
            }
        }

        private void RestoreHUD()
        {
            if (previouslyActiveHUDElements == null) return;

            foreach (var hud in previouslyActiveHUDElements)
            {
                if (hud != null)
                {
                    hud.SetActive(true);
                }
            }
            previouslyActiveHUDElements.Clear();
        }

        #endregion
    }
}
