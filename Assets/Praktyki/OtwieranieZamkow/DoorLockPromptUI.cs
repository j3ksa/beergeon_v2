using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.OtwieranieZamkow
{
    public class DoorLockPromptUI : MonoBehaviour
    {
        public static DoorLockPromptUI Instance { get; private set; }

        [Header("Konfiguracja Fontu")]
        [SerializeField] private TMP_FontAsset customFont;

        [Header("UI Elements")]
        [SerializeField] private Canvas promptCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform promptPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Outline panelOutline;
        [SerializeField] private Image panelBackground;

        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 10f;

        private float targetAlpha = 0f;
        private bool isRequestedThisFrame = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureFontLoaded();
            EnsureUIHierarchy();
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            EnsureFontLoaded();
            ApplyFontToTexts();
        }

        public static void EnsureInstanceExists()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("[DoorLockPromptUI]");
                Instance = obj.AddComponent<DoorLockPromptUI>();
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

        private void ApplyFontToTexts()
        {
            if (customFont == null) return;
            if (titleText != null) titleText.font = customFont;
            if (subtitleText != null) subtitleText.font = customFont;
        }

        public void RequestShowPrompt(bool hasLockpicks, int lockpicksCount)
        {
            EnsureFontLoaded();
            EnsureUIHierarchy();
            isRequestedThisFrame = true;
            targetAlpha = 1f;

            if (titleText != null && subtitleText != null)
            {
                if (hasLockpicks)
                {
                    titleText.text = "<color=#FFD740>[E] UŻYJ WYTRYCHU</color>";
                    subtitleText.text = $"NACIŚNIJ <b>[E]</b>, ABY ROZPOCZĄĆ OTWIERANIE (WYTRYCHY: <color=#FFD740><b>{lockpicksCount}</b></color>)";

                    if (panelOutline != null)
                    {
                        panelOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.85f); // Złotawo-mosiężna obwódka jak w kowadle
                    }
                }
                else
                {
                    titleText.text = "<color=#FF5555>[E] DRZWI ZAMKNIĘTE NA KŁÓDKĘ</color>";
                    subtitleText.text = "ZAMEK JEST ZARYGLOWANY • <color=#FF8888>ZDOBĄDŹ WYTRYCH, ABY OTWORZYĆ DRZWI</color>";

                    if (panelOutline != null)
                    {
                        panelOutline.effectColor = new Color(0.85f, 0.25f, 0.25f, 0.85f); // Szkarłatna obwódka
                    }
                }
            }
        }

        public void HidePromptInstant()
        {
            targetAlpha = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            if (LockpickingMinigameUI.Instance != null && LockpickingMinigameUI.Instance.IsActive)
            {
                targetAlpha = 0f;
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                isRequestedThisFrame = false;
                return;
            }

            if (!isRequestedThisFrame)
            {
                targetAlpha = 0f;
            }
            isRequestedThisFrame = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            }
        }

        public void EnsureUIHierarchy()
        {
            EnsureFontLoaded();

            if (promptCanvas != null && titleText != null && subtitleText != null && canvasGroup != null)
            {
                ApplyFontToTexts();
                return;
            }

            promptCanvas = GetComponentInChildren<Canvas>(true);
            if (promptCanvas == null)
            {
                GameObject canvasObj = new GameObject("DoorLock_Prompt_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(transform, false);
                promptCanvas = canvasObj.GetComponent<Canvas>();
                promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                promptCanvas.sortingOrder = 95;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            canvasGroup = promptCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = promptCanvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            Transform existingPanel = promptCanvas.transform.Find("PromptPanel");
            if (existingPanel != null)
            {
                promptPanel = existingPanel.GetComponent<RectTransform>();
            }
            else
            {
                GameObject panelObj = new GameObject("PromptPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
                panelObj.transform.SetParent(promptCanvas.transform, false);
                promptPanel = panelObj.GetComponent<RectTransform>();

                // Pozycja tuż nad paskiem ekwipunku, idealnie spasowana jak kowadło i sklep
                promptPanel.anchorMin = new Vector2(0.5f, 0.5f);
                promptPanel.anchorMax = new Vector2(0.5f, 0.5f);
                promptPanel.pivot = new Vector2(0.5f, 0.5f);
                promptPanel.sizeDelta = new Vector2(680, 88);
                promptPanel.anchoredPosition = new Vector2(0, -320);

                panelBackground = panelObj.GetComponent<Image>();
                panelBackground.color = new Color(0.06f, 0.06f, 0.08f, 0.94f); // Stylistyka Kowadła

                panelOutline = panelObj.GetComponent<Outline>();
                panelOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.85f);
                panelOutline.effectDistance = new Vector2(2, -2);

                // Tytuł
                titleText = CreateTMP("TitleText", panelObj.transform, 22f, FontStyles.Bold, new Color(1f, 0.84f, 0.25f), new Vector2(0, 16), new Vector2(650, 32));
                titleText.alignment = TextAlignmentOptions.Center;

                // Podtytuł
                subtitleText = CreateTMP("SubtitleText", panelObj.transform, 16f, FontStyles.Normal, Color.white, new Vector2(0, -18), new Vector2(650, 28));
                subtitleText.alignment = TextAlignmentOptions.Center;
            }

            ApplyFontToTexts();
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
    }
}
