using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.SlepySzal
{
    /// <summary>
    /// Efekt winiety tunelowej (Ślepy Szał). 
    /// Całkowicie statyczna, nieprzenikniona czerń bez żadnego pulsowania.
    /// Zmiana wartości w Inspektorze natychmiast odświeża wielkość kółka.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SlepySzalUI : MonoBehaviour
    {
        private static SlepySzalUI instance;

        [Header("Kolor i Wielkość Pola Widzenia")]
        [Tooltip("Głęboka, całkowicie nieprzenikniona czerń otaczająca ekran (brak pulsowania)")]
        [SerializeField] private Color vignetteColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        [Tooltip("Wielkość kółka widzenia (0.1 = małe, 0.25 = średnie, 0.45 = duże, 0.7 = bardzo duże)")]
        [Range(0.02f, 1.00f)] public float innerClearRadius = 0.25f;

        [Tooltip("Zasięg przejścia do 100% czerni (musi być nieco większy niż innerClearRadius)")]
        [Range(0.05f, 1.50f)] public float outerBlackRadius = 0.45f;

        [Tooltip("Szybkość płynnego wejścia i wyjścia winiety")]
        [SerializeField] private float fadeSpeed = 5.0f;

        private Image vignetteImage;
        private float currentAlpha = 0f;
        private bool isEnabled = false;
        private Texture2D generatedTexture;

        private float lastInnerRadius = -1f;
        private float lastOuterRadius = -1f;

        public static SlepySzalUI Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            SetupImageComponent();
        }

        private void OnEnable()
        {
            SlepySzalBuff.OnBuffStateChanged += HandleBuffStateChanged;
        }

        private void OnDisable()
        {
            SlepySzalBuff.OnBuffStateChanged -= HandleBuffStateChanged;
        }

        private void HandleBuffStateChanged(bool active)
        {
            isEnabled = active;
        }

        public static void SetVisionRadius(float clearRadius, float outerRadius)
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<SlepySzalUI>();
            }

            if (instance != null)
            {
                instance.innerClearRadius = clearRadius;
                instance.outerBlackRadius = Mathf.Max(clearRadius + 0.08f, outerRadius);
                instance.RebuildVignetteTexture();
            }
        }

        public static void EnableTunnelVision(bool enable)
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<SlepySzalUI>();
            }

            if (instance == null && enable)
            {
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject uiObj = new GameObject("SlepySzal_UI", typeof(RectTransform));
                    uiObj.transform.SetParent(canvas.transform, false);
                    instance = uiObj.AddComponent<SlepySzalUI>();
                }
            }

            if (instance != null)
            {
                instance.isEnabled = enable;
            }
        }

        private void SetupImageComponent()
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null) parentCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();

            if (parentCanvas != null && transform.parent != parentCanvas.transform)
            {
                transform.SetParent(parentCanvas.transform, false);
            }

            vignetteImage = GetComponent<Image>();
            if (vignetteImage == null)
            {
                vignetteImage = gameObject.AddComponent<Image>();
            }

            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            transform.SetAsFirstSibling();

            vignetteImage.raycastTarget = false;
            RebuildVignetteTexture();

            Color c = vignetteColor;
            c.a = 0f;
            vignetteImage.color = c;
        }

        public void RebuildVignetteTexture()
        {
            if (vignetteImage == null)
            {
                vignetteImage = GetComponent<Image>();
                if (vignetteImage == null) return;
            }

            int size = 256;
            if (generatedTexture == null)
            {
                generatedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                generatedTexture.wrapMode = TextureWrapMode.Clamp;
            }

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxRadius = size / 2f;
            float minOuter = Mathf.Max(innerClearRadius + 0.05f, outerBlackRadius);

            Color[] pixels = new Color[size * size];
            int index = 0;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center.x) / maxRadius;
                    float dy = (y - center.y) / maxRadius;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    if (dist <= innerClearRadius)
                    {
                        alpha = 0f;
                    }
                    else if (dist >= minOuter)
                    {
                        alpha = 1f; // Pełna czerń
                    }
                    else
                    {
                        float t = (dist - innerClearRadius) / (minOuter - innerClearRadius);
                        alpha = Mathf.SmoothStep(0f, 1f, t);
                    }

                    pixels[index++] = new Color(0f, 0f, 0f, alpha);
                }
            }

            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply();

            vignetteImage.sprite = Sprite.Create(generatedTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            vignetteImage.SetAllDirty();

            lastInnerRadius = innerClearRadius;
            lastOuterRadius = outerBlackRadius;
        }

        private void Update()
        {
            // Automatyczne odświeżenie wielkości, jeśli suwaki w Inspektorze zostały ruszone
            if (Mathf.Abs(innerClearRadius - lastInnerRadius) > 0.001f || Mathf.Abs(outerBlackRadius - lastOuterRadius) > 0.001f)
            {
                RebuildVignetteTexture();
            }

            bool active = isEnabled || SlepySzalBuff.IsActive;
            float targetAlpha = active ? vignetteColor.a : 0f;

            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (currentAlpha > 0.005f)
            {
                // CAŁKOWITY BRAK PULSOWANIA - stała czerń
                if (vignetteImage != null)
                {
                    Color c = vignetteColor;
                    c.a = currentAlpha;
                    vignetteImage.color = c;
                    vignetteImage.enabled = true;
                }
            }
            else if (vignetteImage != null && vignetteImage.color.a > 0f)
            {
                currentAlpha = 0f;
                Color c = vignetteColor;
                c.a = 0f;
                vignetteImage.color = c;
            }
        }

        private void OnDestroy()
        {
            if (generatedTexture != null)
            {
                Destroy(generatedTexture);
            }
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
