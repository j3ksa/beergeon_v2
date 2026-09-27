using System;
using UnityEngine;
using UnityEngine.UI;

namespace Praktyki.ByczePaliwo
{
    /// <summary>
    /// Efekt energetycznej żółtej obwódki (Vignette / Błysku Energii) wokół ekranu po wypiciu lub zebraniu Byczego Paliwa.
    /// Skrypt sam generuje proceduralną teksturę i automatycznie podpina się pod aktywny Canvas.
    /// </summary>
    public class ByczePaliwoUI : MonoBehaviour
    {
        public static event Action OnByczePaliwoUsed;

        [Header("Kolor i Wygląd Żółtej Otoczki")]
        [Tooltip("Kolor żółtej obwódki (złocisto-żółty energetyk)")]
        [SerializeField] private Color energyColor = new Color(1.0f, 0.88f, 0.0f, 0.60f);

        [Tooltip("Czas trwania widoczności otoczki w sekundach")]
        [SerializeField] private float flashDuration = 1.8f;

        [Tooltip("Szybkość pulsowania energii wokół ekranu")]
        [SerializeField] private float pulseSpeed = 6.0f;

        [Tooltip("Wielkość czystego środka ekranu (0.55 = 55% środka ekranu jest czyste, a brzegi mają żółtą poświatę)")]
        [SerializeField] [Range(0.1f, 0.9f)] private float innerClearRadius = 0.55f;

        private static ByczePaliwoUI instance;
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

        private void OnEnable()
        {
            OnByczePaliwoUsed += HandleTriggerFlash;
        }

        private void OnDisable()
        {
            OnByczePaliwoUsed -= HandleTriggerFlash;
        }

        /// <summary>
        /// Globalne wywołanie żółtej otoczki wokół ekranu po wypiciu/zebraniu Byczego Paliwa.
        /// Automatycznie tworzy UI na Canvasie jeśli jeszcze nie istnieje w scenie!
        /// </summary>
        public static void TriggerFlash(float duration = 1.8f)
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<ByczePaliwoUI>();
            }

            if (instance == null)
            {
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject uiObj = new GameObject("ByczePaliwo_UI", typeof(RectTransform));
                    uiObj.transform.SetParent(canvas.transform, false);
                    instance = uiObj.AddComponent<ByczePaliwoUI>();
                }
            }

            if (instance != null)
            {
                instance.flashDuration = duration;
                instance.HandleTriggerFlash();
            }
            else
            {
                OnByczePaliwoUsed?.Invoke();
            }
        }

        private void HandleTriggerFlash()
        {
            flashTimer = flashDuration;
            currentAlpha = energyColor.a;

            if (vignetteImage != null)
            {
                Color c = energyColor;
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

            // Ustawiamy pod innymi elementami HUD, aby nie zasłaniało przycisków
            transform.SetAsFirstSibling();

            vignetteImage.raycastTarget = false;
            vignetteImage.sprite = CreateVignetteSprite();

            // Startowo przezroczyste
            Color c = energyColor;
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

                // Efekt lekkiego pulsowania energii
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;
                float targetA = energyColor.a * progress * pulse;

                currentAlpha = Mathf.MoveTowards(currentAlpha, targetA, Time.deltaTime * 3f);

                if (vignetteImage != null)
                {
                    Color c = energyColor;
                    c.a = Mathf.Clamp01(currentAlpha);
                    vignetteImage.color = c;
                }
            }
            else if (currentAlpha > 0.005f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, Time.deltaTime * 4f);
                if (vignetteImage != null)
                {
                    Color c = energyColor;
                    c.a = currentAlpha;
                    vignetteImage.color = c;
                }
            }
            else if (vignetteImage != null && vignetteImage.color.a > 0f)
            {
                currentAlpha = 0f;
                Color c = energyColor;
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
