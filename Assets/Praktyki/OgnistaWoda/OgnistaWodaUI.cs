using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.OgnistaWoda
{
    /// <summary>
    /// Efekt delikatnej, miękkiej szarej ramki/winiety wokół ekranu podczas aktywnego buffa Ognistej Wody.
    /// Skrypt samoczynnie generuje proceduralną teksturę i automatycznie dopina się pod główny Canvas w scenie.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class OgnistaWodaUI : MonoBehaviour
    {
        private static OgnistaWodaUI instance;

        [Header("Kolor i Przezroczystość Szarej Ramki")]
        [Tooltip("Kolor szarej ramki/winiety wokół ekranu (delikatny, miękki szary)")]
        [SerializeField] private Color vignetteColor = new Color(0.60f, 0.62f, 0.66f, 0.20f);

        [Tooltip("Szybkość płynnego pojawiania się i znikania ramki")]
        [SerializeField] private float fadeSpeed = 3.5f;

        [Tooltip("Szybkość subtelnego pulsowania ramki")]
        [SerializeField] private float pulseSpeed = 2.5f;

        [Tooltip("Intensywność pulsowania (bardzo delikatna)")]
        [SerializeField] private float pulseIntensity = 0.025f;

        [Header("Zasięg Ramki")]
        [Tooltip("Wielkość czystego środka ekranu (0.65 = 65% środka ekranu jest w 100% czyste, szary cień pojawia się przy obrzeżach)")]
        [SerializeField] [Range(0.2f, 0.9f)] private float innerClearRadius = 0.65f;

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

        public static void TriggerEffect(float duration = 10.0f)
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<OgnistaWodaUI>();
            }

            if (instance == null)
            {
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject uiObj = new GameObject("OgnistaWoda_UI", typeof(RectTransform));
                    uiObj.transform.SetParent(canvas.transform, false);
                    instance = uiObj.AddComponent<OgnistaWodaUI>();
                }
            }

            if (instance != null)
            {
                instance.activeTimer = duration;
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

            // Umieszczamy w tle pod HUDem
            transform.SetAsFirstSibling();

            vignetteImage.raycastTarget = false;
            vignetteImage.sprite = CreateVignetteSprite();

            // Startowo niewidoczne
            Color c = vignetteColor;
            c.a = 0f;
            vignetteImage.color = c;
        }

        private Sprite CreateVignetteSprite()
        {
            int size = 256;
            generatedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            generatedTexture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxRadius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                    float alpha = 0f;

                    if (dist > innerClearRadius)
                    {
                        float t = (dist - innerClearRadius) / (1.0f - innerClearRadius);
                        alpha = Mathf.Clamp01(Mathf.Pow(t, 2.0f));
                    }

                    generatedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            generatedTexture.Apply();
            return Sprite.Create(generatedTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            bool isBuffActive = OgnistaWodaBuff.IsActive || activeTimer > 0f;
            if (activeTimer > 0f)
            {
                activeTimer -= Time.deltaTime;
            }

            float targetAlpha = isBuffActive ? vignetteColor.a : 0f;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (currentAlpha > 0.005f)
            {
                float pulse = isBuffActive ? Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity : 0f;
                float finalAlpha = Mathf.Clamp01(currentAlpha + pulse);

                if (vignetteImage != null)
                {
                    Color c = vignetteColor;
                    c.a = finalAlpha;
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
