using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.StaryHmielnik
{
    /// <summary>
    /// Delikatna, lekka zielona ramka / winieta wokół ekranu informująca gracza o spowolnieniu szyszką chmielu.
    /// Samoczynnie generuje miękką proceduralną teksturę i podpina się pod główny Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HopSlowVignetteUI : MonoBehaviour
    {
        private static HopSlowVignetteUI instance;

        [Header("Kolor i Przezroczystość Ramki Spowolnienia")]
        [Tooltip("Kolor lekkiej zielonej ramki (subtelny, zgaszony zielony)")]
        [SerializeField] private Color vignetteColor = new Color(0.20f, 0.70f, 0.25f, 0.14f);

        [Tooltip("Szybkość płynnego pojawiania się i znikania ramki")]
        [SerializeField] private float fadeSpeed = 4.0f;

        [Tooltip("Szybkość subtelnego pulsowania ramki")]
        [SerializeField] private float pulseSpeed = 2.0f;

        [Tooltip("Intensywność pulsowania (bardzo delikatna)")]
        [SerializeField] private float pulseIntensity = 0.02f;

        [Header("Zasięg Ramki")]
        [Tooltip("Wielkość czystego środka ekranu (0.7 = 70% środka ekranu jest w 100% czyste)")]
        [SerializeField] [Range(0.3f, 0.9f)] private float innerClearRadius = 0.68f;

        private Image vignetteImage;
        private float currentAlpha = 0f;
        private float activeTimer = 0f;
        private Texture2D generatedTexture;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            SetupImageComponent();
        }

        public static void TriggerEffect(float duration = 2.5f)
        {
            EnsureInstance();

            if (instance != null)
            {
                instance.activeTimer = Mathf.Max(instance.activeTimer, duration);
            }
        }

        public static void StopEffect()
        {
            if (instance != null)
            {
                instance.activeTimer = 0f;
            }
        }

        private static void EnsureInstance()
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<HopSlowVignetteUI>();
            }

            if (instance == null)
            {
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject uiObj = new GameObject("HopSlow_VignetteUI", typeof(RectTransform));
                    uiObj.transform.SetParent(canvas.transform, false);
                    instance = uiObj.AddComponent<HopSlowVignetteUI>();
                }
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

            // Rozciągamy na cały ekran
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

            // W tle pod HUDem
            transform.SetAsFirstSibling();

            vignetteImage.raycastTarget = false;
            GenerateVignetteTexture();

            Color startCol = vignetteColor;
            startCol.a = 0f;
            vignetteImage.color = startCol;
        }

        private void GenerateVignetteTexture()
        {
            int size = 128;
            generatedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            generatedTexture.wrapMode = TextureWrapMode.Clamp;
            generatedTexture.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxDist = size * 0.5f * 1.4142f; // Przekątna

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha = Mathf.InverseLerp(innerClearRadius * 0.7071f, 0.7071f, dist);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    generatedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            generatedTexture.Apply();

            vignetteImage.sprite = Sprite.Create(
                generatedTexture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f)
            );
        }

        private void Update()
        {
            if (vignetteImage == null) return;

            bool isActive = activeTimer > 0f;
            if (isActive)
            {
                activeTimer -= Time.deltaTime;
            }

            float targetAlpha = isActive ? vignetteColor.a : 0f;
            if (isActive && pulseIntensity > 0f)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                targetAlpha = Mathf.Clamp01(targetAlpha + pulse);
            }

            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

            Color c = vignetteColor;
            c.a = currentAlpha;
            vignetteImage.color = c;
            vignetteImage.enabled = currentAlpha > 0.001f;
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
