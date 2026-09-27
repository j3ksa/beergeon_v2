using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Praktyki.OtwieranieZamkow
{
    public class LockpickingMinigameUI : MonoBehaviour
    {
        public static LockpickingMinigameUI Instance { get; private set; }

        [Header("Konfiguracja Fontu")]
        [SerializeField] private TMP_FontAsset customFont;

        [Header("Config")]
        [SerializeField] private int requiredPins = 3;
        [SerializeField] private float baseNeedleSpeed = 1.35f;
        [SerializeField] private float sweetSpotWidthPercent = 0.22f; // 22% szerokości paska
        [SerializeField] private float critSpotWidthPercent = 0.06f;   // 6% szerokości paska

        [Header("UI References")]
        [SerializeField] private Canvas minigameCanvas;
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private RectTransform trackRect;
        [SerializeField] private RectTransform sweetSpotRect;
        [SerializeField] private RectTransform critSpotRect;
        [SerializeField] private RectTransform needleRect;
        [SerializeField] private Image sweetSpotImage;
        [SerializeField] private Image critSpotImage;
        [SerializeField] private Image needleImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI lockpicksCountText;
        [SerializeField] private Image lockpickIconImage;
        [SerializeField] private Image[] pinIndicatorImages;
        [SerializeField] private TextMeshProUGUI[] pinIndicatorTexts;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pinSuccessSound;
        [SerializeField] private AudioClip lockpickBreakSound;
        [SerializeField] private AudioClip unlockSuccessSound;

        // Stan gry
        private bool isMinigameActive = false;
        private int currentPin = 0;
        private float needleProgress = 0f; // 0..1
        private float needleTimer = 0f;
        private float currentSpeed = 1.35f;
        private float sweetSpotCenter = 0.5f; // 0..1

        private Item currentLockpickItem;
        private Action onUnlockSuccess;
        private Action onUnlockFailed;

        public bool IsActive => isMinigameActive;

        private float shakeTimer = 0f;
        private float ignoreInputTimer = 0f;
        private Vector3 panelOriginalPos;
        private Coroutine closeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureFontLoaded();
            EnsureComponents();
            GenerateProceduralAudioClips();
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        private void Start()
        {
            EnsureFontLoaded();
            ApplyFontToAllTexts();
        }

        public static void EnsureInstanceExists()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("[LockpickingMinigameUI]");
                Instance = obj.AddComponent<LockpickingMinigameUI>();
                DontDestroyOnLoad(obj);
            }
        }

        private void EnsureFontLoaded()
        {
            if (customFont != null) return;

#if UNITY_EDITOR
            string[] possiblePaths = new string[]
            {
                "Assets/Praktyki/IbarraRealNova-VariableFont_wght SDF.asset",
                "Assets/Praktyki/SaveSystem/Graphics/IbarraRealNova-VariableFont_wght SDF.asset"
            };

            foreach (string path in possiblePaths)
            {
                TMP_FontAsset font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null)
                {
                    customFont = font;
                    break;
                }
            }
#endif
            if (customFont == null)
            {
                customFont = Resources.Load<TMP_FontAsset>("IbarraRealNova-VariableFont_wght SDF");
            }
        }

        private void ApplyFontToAllTexts()
        {
            if (customFont == null) return;

            if (titleText != null) titleText.font = customFont;
            if (statusText != null) statusText.font = customFont;
            if (lockpicksCountText != null) lockpicksCountText.font = customFont;

            if (pinIndicatorTexts != null)
            {
                foreach (var t in pinIndicatorTexts)
                {
                    if (t != null) t.font = customFont;
                }
            }

            // Przeszukaj wszystkie teksty wewnątrz rootPanel
            if (rootPanel != null)
            {
                var allTMP = rootPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in allTMP)
                {
                    if (tmp != null) tmp.font = customFont;
                }
            }
        }

        public void StartMinigame(Item lockpickItem, Action onSuccess, Action onFail)
        {
            EnsureFontLoaded();
            EnsureComponents();

            currentLockpickItem = lockpickItem != null ? lockpickItem : Resources.Load<Item>("Items/Wytrych");
            if (currentLockpickItem == null) currentLockpickItem = Resources.Load<Item>("Wytrych");

            int available = 0;
            if (Inventory.instance != null)
            {
                if (currentLockpickItem != null) available = Inventory.instance.GetTotalItemCount(currentLockpickItem);
                if (available <= 0)
                {
                    for (int i = 0; i < Inventory.instance.items.Count; i++)
                    {
                        var itm = Inventory.instance.items[i];
                        if (itm != null && (itm.itemName.ToLower().Contains("wytrych") || itm.name.ToLower().Contains("wytrych")))
                        {
                            currentLockpickItem = itm;
                            available = Inventory.instance.GetItemCount(i);
                            break;
                        }
                    }
                }
            }

            if (available <= 0)
            {
                Debug.LogWarning("[LockpickingMinigame] Brak wytrychów w ekwipunku!");
                onFail?.Invoke();
                return;
            }

            onUnlockSuccess = onSuccess;
            onUnlockFailed = onFail;

            currentPin = 0;
            needleTimer = 0f;
            currentSpeed = baseNeedleSpeed;
            isMinigameActive = true;
            ignoreInputTimer = 0.22f;

            // Zatrzymanie czasu podczas minigry
            Time.timeScale = 0f;

            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
                panelOriginalPos = rootPanel.transform.localPosition;
            }

            PickNewSweetSpot();
            UpdateUI();
            ApplyFontToAllTexts();

            if (statusText != null)
            {
                statusText.text = "TRAF W ZIELONE POLE, ABY PODWAŻYĆ ZAPADKĘ ZAMKA";
                statusText.color = new Color(0.9f, 0.9f, 0.9f);
            }
        }

        public void CloseMinigame(bool success)
        {
            isMinigameActive = false;
            UIOverlayManager.NotifyOverlayClosed();
            Time.timeScale = 1f;

            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (success)
            {
                onUnlockSuccess?.Invoke();
            }
            else
            {
                onUnlockFailed?.Invoke();
            }

            onUnlockSuccess = null;
            onUnlockFailed = null;
        }

        private void Update()
        {
            if (!isMinigameActive) return;

            // ESC do wyjścia
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseMinigame(false);
                return;
            }

            // Animacja wstrząsu panelu przy błędzie
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.unscaledDeltaTime;
                if (rootPanel != null)
                {
                    Vector3 offset = UnityEngine.Random.insideUnitCircle * 8f;
                    rootPanel.transform.localPosition = panelOriginalPos + offset;
                    if (shakeTimer <= 0f) rootPanel.transform.localPosition = panelOriginalPos;
                }
            }

            // Ruch iglicy/wskaźnika
            needleTimer += Time.unscaledDeltaTime * currentSpeed;
            needleProgress = Mathf.PingPong(needleTimer, 1f);

            UpdateNeedleVisualPosition();

            // Krótkie opóźnienie ochronne (debounce), by wciśnięcie [E] otwierające minigrę nie było traktowane jako natychmiastowa próba
            if (ignoreInputTimer > 0f)
            {
                ignoreInputTimer -= Time.unscaledDeltaTime;
                return;
            }

            // Odczyt inputu gracza (Spacja, E lub LPM)
            bool attempt = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
                    attempt = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                attempt = true;
            }

            if (attempt)
            {
                EvaluatePickAttempt();
            }
        }

        private void UpdateNeedleVisualPosition()
        {
            if (trackRect == null || needleRect == null) return;

            float trackWidth = trackRect.rect.width;
            float posX = (needleProgress - 0.5f) * trackWidth;
            needleRect.anchoredPosition = new Vector2(posX, 0f);
        }

        private void PickNewSweetSpot()
        {
            float margin = sweetSpotWidthPercent * 0.65f;
            sweetSpotCenter = UnityEngine.Random.Range(margin, 1f - margin);

            if (trackRect == null || sweetSpotRect == null) return;

            float trackWidth = trackRect.rect.width;
            float sweetWidth = trackWidth * sweetSpotWidthPercent;
            float critWidth = trackWidth * critSpotWidthPercent;

            sweetSpotRect.sizeDelta = new Vector2(sweetWidth, trackRect.rect.height);
            float posX = (sweetSpotCenter - 0.5f) * trackWidth;
            sweetSpotRect.anchoredPosition = new Vector2(posX, 0f);

            if (critSpotRect != null)
            {
                critSpotRect.sizeDelta = new Vector2(critWidth, trackRect.rect.height);
                critSpotRect.anchoredPosition = Vector2.zero;
            }
        }

        private void EvaluatePickAttempt()
        {
            float halfSweet = sweetSpotWidthPercent * 0.5f;
            float halfCrit = critSpotWidthPercent * 0.5f;
            float diff = Mathf.Abs(needleProgress - sweetSpotCenter);

            if (diff <= halfSweet)
            {
                // SUKCES!
                bool isCrit = (diff <= halfCrit);
                currentPin++;
                PlaySound(pinSuccessSound, isCrit ? 1.25f : 1.0f);

                if (isCrit)
                {
                    if (statusText != null)
                    {
                        statusText.text = "<color=#FFD740><b>★ IDEALNY RUCH! ZAPADKA WSKOCZYŁA! ★</b></color>";
                    }
                }
                else
                {
                    if (statusText != null)
                    {
                        statusText.text = "<color=#2ECC71><b>KLIK! ZAPADKA PODWAŻONA!</b></color>";
                    }
                }

                if (currentPin >= requiredPins)
                {
                    PlaySound(unlockSuccessSound, 1.0f);
                    if (statusText != null)
                    {
                        statusText.text = "<color=#00FF7F><b>★ ZAMEK ROZBROJONY! KŁÓDKA OTWARTA! ★</b></color>";
                    }
                    UpdateUI();
                    if (closeRoutine != null) StopCoroutine(closeRoutine);
                    closeRoutine = StartCoroutine(DelayedClose(true, 0.45f));
                    return;
                }

                currentSpeed = baseNeedleSpeed + (currentPin * 0.35f);
                PickNewSweetSpot();
                UpdateUI();
            }
            else
            {
                // BŁĄD! Wytrych pęka
                shakeTimer = 0.25f;
                PlaySound(lockpickBreakSound, 0.95f);

                if (Inventory.instance != null && currentLockpickItem != null)
                {
                    Inventory.instance.ConsumeItem(currentLockpickItem, 1);
                }

                int left = Inventory.instance != null && currentLockpickItem != null ? Inventory.instance.GetTotalItemCount(currentLockpickItem) : 0;
                UpdateUI();

                if (left <= 0)
                {
                    if (statusText != null)
                    {
                        statusText.text = "<color=#FF4444><b>TRZASK! WYTRYCH PĘKŁ! BRAK WIĘCEJ WYTRYCHÓW!</b></color>";
                    }
                    if (closeRoutine != null) StopCoroutine(closeRoutine);
                    closeRoutine = StartCoroutine(DelayedClose(false, 0.9f));
                }
                else
                {
                    if (statusText != null)
                    {
                        statusText.text = $"<color=#FF5555>TRZASK! ZŁAMAŁEŚ WYTRYCH! POZOSTAŁO: {left}</color>";
                    }
                    if (currentPin > 0) currentPin--;
                    PickNewSweetSpot();
                    UpdateUI();
                }
            }
        }

        private IEnumerator DelayedClose(bool success, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            CloseMinigame(success);
        }

        private void UpdateUI()
        {
            int left = Inventory.instance != null && currentLockpickItem != null ? Inventory.instance.GetTotalItemCount(currentLockpickItem) : 0;

            if (lockpicksCountText != null)
            {
                lockpicksCountText.text = $"WYTRYCHY: <color=#FFD740><b>{left}</b></color>";
            }

            if (pinIndicatorImages != null)
            {
                for (int i = 0; i < pinIndicatorImages.Length; i++)
                {
                    if (pinIndicatorImages[i] != null)
                    {
                        bool isSolved = (i < currentPin);
                        pinIndicatorImages[i].color = isSolved 
                            ? new Color(0.18f, 0.85f, 0.45f, 0.95f) 
                            : new Color(0.14f, 0.15f, 0.18f, 0.85f);
                    }
                }
            }

            if (pinIndicatorTexts != null)
            {
                for (int i = 0; i < pinIndicatorTexts.Length; i++)
                {
                    if (pinIndicatorTexts[i] != null)
                    {
                        bool isSolved = (i < currentPin);
                        pinIndicatorTexts[i].color = isSolved ? new Color(0.05f, 0.1f, 0.05f, 1f) : new Color(0.7f, 0.7f, 0.7f, 0.8f);
                    }
                }
            }

            if (lockpickIconImage != null && currentLockpickItem != null && currentLockpickItem.icon != null)
            {
                lockpickIconImage.sprite = currentLockpickItem.icon;
            }
        }

        private void PlaySound(AudioClip clip, float pitch = 1f)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(clip, 0.85f);
            }
        }

        private void EnsureComponents()
        {
            EnsureFontLoaded();

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.ignoreListenerPause = true;
            }

            // Usuń ewentualny stary panel o złych wymiarach, by zbudować go w pełnej stylistyce kowadła
            if (minigameCanvas != null && rootPanel != null)
            {
                var existingHeader = rootPanel.transform.Find("DividerLine");
                if (existingHeader != null)
                {
                    ApplyFontToAllTexts();
                    return;
                }
                else
                {
                    DestroyImmediate(rootPanel);
                    rootPanel = null;
                }
            }

            // Tworzymy ScreenSpace Canvas jeśli nie istnieje
            minigameCanvas = GetComponentInChildren<Canvas>(true);
            if (minigameCanvas == null)
            {
                GameObject canvasObj = new GameObject("Lockpicking_ScreenHUD_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(transform, false);
                minigameCanvas = canvasObj.GetComponent<Canvas>();
                minigameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                minigameCanvas.sortingOrder = 120;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Panel główny w stylistyce Kowadła
            GameObject panelObj = new GameObject("PromptPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            panelObj.transform.SetParent(minigameCanvas.transform, false);
            rootPanel = panelObj;

            RectTransform pRect = panelObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(720, 390);
            pRect.anchoredPosition = new Vector2(0, 0);

            // Ciemny, elegancki medieval podkład jak w kowadle
            Image pImg = panelObj.GetComponent<Image>();
            pImg.color = new Color(0.06f, 0.06f, 0.08f, 0.95f);

            // Złotawo-mosiężna obwódka
            Outline outline = panelObj.GetComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.85f);
            outline.effectDistance = new Vector2(2, -2);

            // 1. Tytuł: Ibarra Real Nova ALL CAPS
            titleText = CreateTMP("TitleText", panelObj.transform, 28f, FontStyles.Bold, new Color(1f, 0.84f, 0.25f), new Vector2(0, 150), new Vector2(680, 40));
            titleText.text = "OTWIERANIE ZAMKA";
            titleText.alignment = TextAlignmentOptions.Center;

            // 2. Ozdobna złota linia oddzielająca nagłówek
            GameObject divider = new GameObject("DividerLine", typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(panelObj.transform, false);
            RectTransform divRect = divider.GetComponent<RectTransform>();
            divRect.anchoredPosition = new Vector2(0, 122);
            divRect.sizeDelta = new Vector2(620, 2);
            Image divImg = divider.GetComponent<Image>();
            divImg.color = new Color(0.85f, 0.70f, 0.30f, 0.5f);

            // 3. Rząd informacyjny: Wytrychy (Lewa strona)
            GameObject lpBadge = new GameObject("LockpickBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
            lpBadge.transform.SetParent(panelObj.transform, false);
            RectTransform lpBadgeRect = lpBadge.GetComponent<RectTransform>();
            lpBadgeRect.anchoredPosition = new Vector2(-190, 85);
            lpBadgeRect.sizeDelta = new Vector2(230, 42);
            Image lpBadgeImg = lpBadge.GetComponent<Image>();
            lpBadgeImg.color = new Color(0.03f, 0.04f, 0.05f, 0.85f);
            Outline lpOutline = lpBadge.GetComponent<Outline>();
            lpOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.4f);
            lpOutline.effectDistance = new Vector2(1, -1);

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(lpBadge.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(-75, 0);
            iconRect.sizeDelta = new Vector2(32, 32);
            lockpickIconImage = iconObj.GetComponent<Image>();

            lockpicksCountText = CreateTMP("CountText", lpBadge.transform, 19f, FontStyles.Bold, Color.white, new Vector2(30, 0), new Vector2(160, 36));
            lockpicksCountText.alignment = TextAlignmentOptions.Left;

            // 4. Trzy zapadki (Prawa strona - kafelki w stylu kowadła)
            GameObject pinsContainer = new GameObject("PinsContainer", typeof(RectTransform));
            pinsContainer.transform.SetParent(panelObj.transform, false);
            RectTransform pinsContRect = pinsContainer.GetComponent<RectTransform>();
            pinsContRect.anchoredPosition = new Vector2(170, 85);
            pinsContRect.sizeDelta = new Vector2(270, 42);

            pinIndicatorImages = new Image[requiredPins];
            pinIndicatorTexts = new TextMeshProUGUI[requiredPins];
            string[] romanNumbers = new string[] { "I", "II", "III" };

            for (int i = 0; i < requiredPins; i++)
            {
                GameObject pinPlate = new GameObject($"PinPlate_{i}", typeof(RectTransform), typeof(Image), typeof(Outline));
                pinPlate.transform.SetParent(pinsContainer.transform, false);
                RectTransform plateRect = pinPlate.GetComponent<RectTransform>();
                plateRect.anchoredPosition = new Vector2((i - 1) * 88, 0);
                plateRect.sizeDelta = new Vector2(76, 36);

                Image plateImg = pinPlate.GetComponent<Image>();
                plateImg.color = new Color(0.14f, 0.15f, 0.18f, 0.85f);
                pinIndicatorImages[i] = plateImg;

                Outline plateOutline = pinPlate.GetComponent<Outline>();
                plateOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.4f);
                plateOutline.effectDistance = new Vector2(1, -1);

                TextMeshProUGUI pText = CreateTMP("Text", pinPlate.transform, 16f, FontStyles.Bold, new Color(0.7f, 0.7f, 0.7f), Vector2.zero, new Vector2(76, 36));
                pText.text = $"ZAP. {romanNumbers[i]}";
                pText.alignment = TextAlignmentOptions.Center;
                pinIndicatorTexts[i] = pText;
            }

            // 5. Główny mechanizm bębenka (Timing Track)
            GameObject trackFrame = new GameObject("TrackFrame", typeof(RectTransform), typeof(Image), typeof(Outline));
            trackFrame.transform.SetParent(panelObj.transform, false);
            RectTransform tfRect = trackFrame.GetComponent<RectTransform>();
            tfRect.anchoredPosition = new Vector2(0, -10);
            tfRect.sizeDelta = new Vector2(580, 52);
            Image tfImg = trackFrame.GetComponent<Image>();
            tfImg.color = new Color(0.02f, 0.03f, 0.04f, 0.98f);
            Outline tfOutline = trackFrame.GetComponent<Outline>();
            tfOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.65f);
            tfOutline.effectDistance = new Vector2(1, -1);

            GameObject trackObj = new GameObject("Track", typeof(RectTransform));
            trackObj.transform.SetParent(trackFrame.transform, false);
            trackRect = trackObj.GetComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.sizeDelta = new Vector2(-12, -12); // Wewnętrzny margines
            trackRect.anchoredPosition = Vector2.zero;

            // Sweet Spot (Szmaragdowa strefa sukcesu)
            GameObject sweetObj = new GameObject("SweetSpot", typeof(RectTransform), typeof(Image), typeof(Outline));
            sweetObj.transform.SetParent(trackRect.transform, false);
            sweetSpotRect = sweetObj.GetComponent<RectTransform>();
            sweetSpotRect.anchorMin = new Vector2(0.5f, 0f);
            sweetSpotRect.anchorMax = new Vector2(0.5f, 1f);
            sweetSpotRect.pivot = new Vector2(0.5f, 0.5f);
            sweetSpotRect.sizeDelta = new Vector2(110, 40);
            sweetSpotImage = sweetObj.GetComponent<Image>();
            sweetSpotImage.color = new Color(0.18f, 0.85f, 0.45f, 0.92f);
            Outline sweetOutline = sweetObj.GetComponent<Outline>();
            sweetOutline.effectColor = new Color(0.4f, 1f, 0.6f, 0.8f);
            sweetOutline.effectDistance = new Vector2(1, -1);

            // Crit Spot (Środkowy złoty rdzeń)
            GameObject critObj = new GameObject("CritSpot", typeof(RectTransform), typeof(Image));
            critObj.transform.SetParent(sweetObj.transform, false);
            critSpotRect = critObj.GetComponent<RectTransform>();
            critSpotRect.anchorMin = new Vector2(0.5f, 0f);
            critSpotRect.anchorMax = new Vector2(0.5f, 1f);
            critSpotRect.pivot = new Vector2(0.5f, 0.5f);
            critSpotRect.sizeDelta = new Vector2(28, 40);
            critSpotImage = critObj.GetComponent<Image>();
            critSpotImage.color = new Color(1f, 0.84f, 0.20f, 1f);

            // Iglica / Wytrych (Needle)
            GameObject needleObj = new GameObject("Needle", typeof(RectTransform), typeof(Image), typeof(Outline));
            needleObj.transform.SetParent(trackRect.transform, false);
            needleRect = needleObj.GetComponent<RectTransform>();
            needleRect.anchorMin = new Vector2(0.5f, 0.5f);
            needleRect.anchorMax = new Vector2(0.5f, 0.5f);
            needleRect.pivot = new Vector2(0.5f, 0.5f);
            needleRect.sizeDelta = new Vector2(8, 64);
            needleImage = needleObj.GetComponent<Image>();
            needleImage.color = new Color(1f, 0.35f, 0.15f, 1f);
            Outline needleOutline = needleObj.GetComponent<Outline>();
            needleOutline.effectColor = new Color(1f, 0.9f, 0.5f, 0.9f);
            needleOutline.effectDistance = new Vector2(1, -1);

            // 6. Dynamiczny status w Ibarra Real Nova
            statusText = CreateTMP("StatusText", panelObj.transform, 19f, FontStyles.Bold, Color.white, new Vector2(0, -78), new Vector2(660, 36));
            statusText.text = "TRAF W ZIELONE POLE, ABY PODWAŻYĆ ZAPADKĘ ZAMKA";
            statusText.alignment = TextAlignmentOptions.Center;

            // 7. Dolny pasek podpowiedzi
            GameObject footerLine = new GameObject("FooterDivider", typeof(RectTransform), typeof(Image));
            footerLine.transform.SetParent(panelObj.transform, false);
            RectTransform flRect = footerLine.GetComponent<RectTransform>();
            flRect.anchoredPosition = new Vector2(0, -118);
            flRect.sizeDelta = new Vector2(620, 1);
            Image flImg = footerLine.GetComponent<Image>();
            flImg.color = new Color(0.85f, 0.70f, 0.30f, 0.3f);

            TextMeshProUGUI hints = CreateTMP("HintsText", panelObj.transform, 16f, FontStyles.Normal, new Color(0.8f, 0.8f, 0.85f), new Vector2(0, -145), new Vector2(660, 30));
            hints.text = "<color=#FFD740>[SPACJA]</color> lub <color=#FFD740>[E]</color> - PRZEKRĘĆ WYTRYCH   •   <color=#FF5555>[ESC]</color> - ANULUJ";
            hints.alignment = TextAlignmentOptions.Center;

            ApplyFontToAllTexts();
        }

        private TextMeshProUGUI CreateTMP(string name, Transform parent, float fontSize, FontStyles style, Color color, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (customFont != null) tmp.font = customFont;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private void GenerateProceduralAudioClips()
        {
            if (pinSuccessSound == null) pinSuccessSound = CreateToneClip("LockPinSuccess", 980f, 0.08f, 0.35f);
            if (lockpickBreakSound == null) lockpickBreakSound = CreateNoiseClip("LockpickBreak", 0.18f, 0.45f);
            if (unlockSuccessSound == null) unlockSuccessSound = CreateChimeClip("LockUnlocked", 0.35f);
        }

        private AudioClip CreateToneClip(string clipName, float frequency, float duration, float gain)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * gain;
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateNoiseClip(string clipName, float duration, float gain)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float envelope = 1f - ((float)i / samples);
                data[i] = (UnityEngine.Random.Range(-1f, 1f) * 0.7f + Mathf.Sin(i * 0.1f) * 0.3f) * envelope * gain;
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChimeClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / samples);
                float wave = Mathf.Sin(2f * Mathf.PI * 523.25f * t) * 0.4f
                           + Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.35f
                           + Mathf.Sin(2f * Mathf.PI * 783.99f * t) * 0.25f;
                data[i] = wave * envelope * 0.5f;
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
