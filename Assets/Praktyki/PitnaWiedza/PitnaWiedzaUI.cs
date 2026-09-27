using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.PitnaWiedza
{
    /// <summary>
    /// Efekt złotej poświaty/winiety (Złoty Rozbłysk Wiedzy / Level Up) wokół ekranu po wypiciu lub zebraniu Pitnej Wiedzy.
    /// Skrypt automatycznie generuje proceduralną teksturę i podpina się pod aktywny Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PitnaWiedzaUI : MonoBehaviour
    {
        private static PitnaWiedzaUI instance;

        [Header("Kolor i Wygląd Złotej Poświaty")]
        [Tooltip("Złoty kolor poświaty wokół ekranu")]
        [SerializeField] private Color goldColor = new Color(1.0f, 0.84f, 0.0f, 0.55f);

        [Tooltip("Czas trwania rozbłysku w sekundach")]
        [SerializeField] private float flashDuration = 2.0f;

        [Tooltip("Szybkość pulsowania złotej aury")]
        [SerializeField] private float pulseSpeed = 4.0f;

        [Tooltip("Wielkość czystego środka ekranu (0.60 = 60% środka ekranu jest czyste)")]
        [SerializeField] [Range(0.2f, 0.9f)] private float innerClearRadius = 0.60f;

        private Image vignetteImage;
        private float currentAlpha = 0f;
        private float flashTimer = 0f;
        private Texture2D generatedTexture;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            SetupImageComponent();
        }

        public static void TriggerFlash(float duration = 2.0f)
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<PitnaWiedzaUI>();
            }

            if (instance == null)
            {
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject uiObj = new GameObject("PitnaWiedza_UI", typeof(RectTransform));
                    uiObj.transform.SetParent(canvas.transform, false);
                    instance = uiObj.AddComponent<PitnaWiedzaUI>();
                }
            }

            if (instance != null)
            {
                instance.flashDuration = duration;
                instance.HandleTriggerFlash();
            }
        }

        private void HandleTriggerFlash()
        {
            flashTimer = flashDuration;
            currentAlpha = goldColor.a;

            if (vignetteImage != null)
            {
                Color c = goldColor;
                c.a = currentAlpha;
                vignetteImage.color = c;
                vignetteImage.enabled = true;
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

            transform.SetAsFirstSibling();

            vignetteImage.raycastTarget = false;
            vignetteImage.sprite = CreateVignetteSprite();

            Color c = goldColor;
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
                        alpha = Mathf.Clamp01(Mathf.Pow(t, 2.2f));
                    }

                    generatedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            generatedTexture.Apply();
            return Sprite.Create(generatedTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                float progress = Mathf.Clamp01(flashTimer / flashDuration);

                // Efekt energetycznego pulsowania złotej poświaty
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;
                float targetA = goldColor.a * progress * pulse;

                currentAlpha = Mathf.MoveTowards(currentAlpha, targetA, Time.deltaTime * 3f);

                if (vignetteImage != null)
                {
                    Color c = goldColor;
                    c.a = Mathf.Clamp01(currentAlpha);
                    vignetteImage.color = c;
                }
            }
            else if (currentAlpha > 0.005f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, Time.deltaTime * 4f);
                if (vignetteImage != null)
                {
                    Color c = goldColor;
                    c.a = currentAlpha;
                    vignetteImage.color = c;
                }
            }
            else if (vignetteImage != null && vignetteImage.color.a > 0f)
            {
                currentAlpha = 0f;
                Color c = goldColor;
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
