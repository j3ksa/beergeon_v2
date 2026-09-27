using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.MlekoStarego
{
    /// <summary>
    /// Efekt wizualny oczyszczenia oraz delikatnej mleczno-białej aury ochronnej wokół ekranu.
    /// Przy wypiciu daje krótki, świeży biały rozbłysk (oczyszczenie), a przez kolejne 10s
    /// utrzymuje subtelny mleczny blask na krawędziach ekranu, sygnalizując aktywną ochronę.
    /// Skrypt samoczynnie generuje teksturę i podpina się pod Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MlekoStaregoUI : MonoBehaviour
    {
        private static MlekoStaregoUI instance;

        [Header("Kolor i Wygląd Mlecznej Aury")]
        [Tooltip("Kolor mlecznej aury na krawędziach ekranu")]
        [SerializeField] private Color auraColor = new Color(0.95f, 0.97f, 1.0f, 0.18f);

        [Tooltip("Szybkość pojawiania się i znikania aury")]
        [SerializeField] private float fadeSpeed = 3.5f;

        [Tooltip("Szybkość pulsowania aury")]
        [SerializeField] private float pulseSpeed = 2.0f;

        [Tooltip("Intensywność pulsowania")]
        [SerializeField] private float pulseIntensity = 0.03f;

        [Header("Zasięg Aury")]
        [Tooltip("Wielkość czystego środka ekranu")]
        [SerializeField] [Range(0.2f, 0.9f)] private float innerClearRadius = 0.68f;

        private Image auraImage;
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
                instance = UnityEngine.Object.FindFirstObjectByType<MlekoStaregoUI>();
            }

            if (instance == null)
            {
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject uiObj = new GameObject("MlekoStarego_UI", typeof(RectTransform));
                    uiObj.transform.SetParent(canvas.transform, false);
                    instance = uiObj.AddComponent<MlekoStaregoUI>();
                }
            }

            if (instance != null)
            {
                instance.activeTimer = duration;
                // Krótki rozbłysk oczyszczenia (początkowe podbicie alfy)
                instance.currentAlpha = Mathf.Max(instance.currentAlpha, 0.45f);
            }
        }

        public static void StopEffect()
        {
            if (instance != null)
            {
                instance.activeTimer = 0f;
            }
        }

        private void SetupImageComponent()
        {
            auraImage = GetComponent<Image>();
            if (auraImage == null)
            {
                auraImage = gameObject.AddComponent<Image>();
            }

            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            auraImage.raycastTarget = false;

            if (generatedTexture == null)
            {
                generatedTexture = CreateMilkyAuraTexture(256, 256, innerClearRadius);
            }

            Sprite auraSprite = Sprite.Create(
                generatedTexture,
                new Rect(0, 0, generatedTexture.width, generatedTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            auraImage.sprite = auraSprite;
            auraImage.type = Image.Type.Simple;
            auraImage.color = new Color(auraColor.r, auraColor.g, auraColor.b, 0f);
        }

        private Texture2D CreateMilkyAuraTexture(int width, int height, float clearRadius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float maxDist = center.magnitude;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normDist = dist / maxDist;

                    float alpha = 0f;
                    if (normDist > clearRadius)
                    {
                        float t = (normDist - clearRadius) / (1f - clearRadius);
                        alpha = Mathf.SmoothStep(0f, 1f, t);
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return tex;
        }

        private void Update()
        {
            if (auraImage == null) return;

            float targetAlpha = 0f;

            if (activeTimer > 0f)
            {
                activeTimer -= Time.deltaTime;

                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                targetAlpha = Mathf.Clamp01(auraColor.a + pulse);
            }

            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

            Color c = auraColor;
            c.a = currentAlpha;
            auraImage.color = c;
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
