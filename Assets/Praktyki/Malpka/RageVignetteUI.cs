using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.Malpka
{
    /// <summary>
    /// Efekt czerwonej obwódki (Vignette / Szał) na ekranie podczas aktywnego buffa z Małpki.
    /// Umieść ten skrypt na Canvasie UI lub na obiekcie z Image.
    /// Skrypt automatycznie wygeneruje lub użyje czerwonej obwódki z efektem tętniącego szału.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RageVignetteUI : MonoBehaviour
    {
        [Header("Kolory i Efekt Szału")]
        [SerializeField] private Color vignetteColor = new Color(0.8f, 0.0f, 0.0f, 0.10f);
        [SerializeField] private float fadeSpeed = 4.0f;
        [SerializeField] private float pulseSpeed = 3.5f;
        [SerializeField] private float pulseIntensity = 0.03f;

        [Header("Opcjonalna własna grafika vignetty")]
        [SerializeField] private Sprite customVignetteSprite;

        private Image vignetteImage;
        private PlayerStats playerStats;
        private float currentAlpha = 0f;
        private Texture2D generatedTexture;

        private void Awake()
        {
            SetupImageComponent();
        }

        private void Start()
        {
            playerStats = Object.FindFirstObjectByType<PlayerStats>();
        }

        private void SetupImageComponent()
        {
            // Znajdujemy główny Canvas i przepinamy obiekt bezpośrednio pod niego,
            // dzięki czemu rozciąga się na CAŁY EKRAN zamiast na mały kontener PotionEffects
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null && transform.parent != parentCanvas.transform)
            {
                transform.SetParent(parentCanvas.transform, false);
            }

            vignetteImage = GetComponent<Image>();
            if (vignetteImage == null)
            {
                vignetteImage = gameObject.AddComponent<Image>();
            }

            // Rozciągamy Image na cały ekran
            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            // Ustawiamy jako tło UI pod innymi elementami HUD
            transform.SetAsFirstSibling();

            // Blokujemy przechwytywanie kliknięć Myszy/Raycast
            vignetteImage.raycastTarget = false;

            if (customVignetteSprite != null)
            {
                vignetteImage.sprite = customVignetteSprite;
            }
            else
            {
                // Generujemy miękką teksturę vignetty prosto z kodu!
                vignetteImage.sprite = CreateVignetteSprite();
            }

            // Startowo przezroczysta
            Color c = vignetteColor;
            c.a = 0f;
            vignetteImage.color = c;
        }

        [Header("Zasięg Vignetty (Krawędzie)")]
        [Tooltip("Wielkość czystego środka ekranu (0.7 oznacza że 70% środka ekranu jest idealnie przezroczysta, a czerwień pojawia się tylko przy ramkach)")]
        [SerializeField] [Range(0.1f, 0.9f)] private float innerClearRadius = 0.70f;

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
                        alpha = Mathf.Clamp01(Mathf.Pow(t, 2.5f));
                    }

                    generatedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            generatedTexture.Apply();
            return Sprite.Create(generatedTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            if (playerStats == null)
            {
                playerStats = Object.FindFirstObjectByType<PlayerStats>();
                if (playerStats == null) return;
            }

            bool isActive = playerStats.IsBuffActive;
            float targetAlpha = isActive ? vignetteColor.a : 0f;

            // Płynne wejście / wyjście
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (currentAlpha > 0.01f)
            {
                // Efekt pulsowania podczas szału
                float pulse = isActive ? Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity : 0f;
                float finalAlpha = Mathf.Clamp01(currentAlpha + pulse);

                Color c = vignetteColor;
                c.a = finalAlpha;
                vignetteImage.color = c;
            }
            else
            {
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
        }
    }
}
