using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.SpeedPotion
{
    /// <summary>
    /// Efekt niebieskiej obwódki (Vignette / Powiew Wiatru) na ekranie podczas aktywnego buffa prędkości z Jagerka.
    /// Umieść ten skrypt na Canvasie UI lub na dowolnym obiekcie w scenie (sam się podepnie i stworzy obraz).
    /// </summary>
    public class SpeedBuffUI : MonoBehaviour
    {
        [Header("Kolory i Efekt Przyspieszenia")]
        [SerializeField] private Color effectColor = new Color(0.0f, 0.5f, 1.0f, 0.15f); // Jasnoniebieski
        [SerializeField] private float fadeSpeed = 5.0f;
        [SerializeField] private float pulseSpeed = 4.0f;
        [SerializeField] private float pulseIntensity = 0.05f;

        [Header("Zasięg Vignetty (Krawędzie)")]
        [Tooltip("Wielkość czystego środka ekranu (0.6 oznacza że 60% środka ekranu jest idealnie przezroczysta)")]
        [SerializeField] [Range(0.1f, 0.9f)] private float innerClearRadius = 0.65f;

        private Image vignetteImage;
        private PlayerSpeedBuff playerSpeedBuff;
        private float currentAlpha = 0f;
        private Texture2D generatedTexture;

        private void Awake()
        {
            SetupImageComponent();
        }

        private void Start()
        {
            playerSpeedBuff = Object.FindFirstObjectByType<PlayerSpeedBuff>();
        }

        private void SetupImageComponent()
        {
            // Znajdujemy główny Canvas i przepinamy obiekt bezpośrednio pod niego,
            // dzięki czemu rozciąga się na CAŁY EKRAN tak samo jak w RageVignetteUI (Małpka)
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null) parentCanvas = Object.FindFirstObjectByType<Canvas>();

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

            // Generujemy miękką teksturę vignetty z kodu (nie trzeba podpinać Sprite'a!)
            vignetteImage.sprite = CreateVignetteSprite();

            // Startowo przezroczysta
            Color c = effectColor;
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
                        alpha = Mathf.Clamp01(Mathf.Pow(t, 2.5f));
                    }

                    // Tworzymy przezroczysto-białą maskę, która zostanie zabarwiona przez effectColor
                    generatedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            generatedTexture.Apply();
            return Sprite.Create(generatedTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            if (playerSpeedBuff == null)
            {
                playerSpeedBuff = Object.FindFirstObjectByType<PlayerSpeedBuff>();
                if (playerSpeedBuff == null) return;
            }

            bool isActive = playerSpeedBuff.IsBuffActive;
            float targetAlpha = isActive ? effectColor.a : 0f;

            // Płynne wejście / wyjście
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (currentAlpha > 0.01f)
            {
                // Efekt pulsowania wiatru
                float pulse = isActive ? Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity : 0f;
                float finalAlpha = Mathf.Clamp01(currentAlpha + pulse);

                Color c = effectColor;
                c.a = finalAlpha;
                vignetteImage.color = c;
            }
            else
            {
                Color c = effectColor;
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
