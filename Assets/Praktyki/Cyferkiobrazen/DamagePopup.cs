using TMPro;
using UnityEngine;

namespace DamageNumbers
{
    /// <summary>
    /// Pojedynczy unoszący się napis obrażeń z soczystą animacją pop/scale,
    /// łukiem ruchu, fade-outem oraz billboardem skierowanym do kamery.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TMP_Text textMesh;

        [Header("Animation Settings")]
        [SerializeField] private float lifetime = 0.85f;
        [SerializeField] private float floatSpeed = 2.4f;
        [SerializeField] private float deceleration = 1.8f;
        [SerializeField] private float horizontalSpread = 0.7f;

        private float currentLifetime;
        private Vector3 moveVelocity;
        private Camera mainCam;
        private Color baseColor;
        private float targetBaseScale = 1f;
        private float punchScaleMultiplier = 1.35f;

        private void Awake()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TMP_Text>();
                if (textMesh == null)
                {
                    textMesh = GetComponentInChildren<TMP_Text>(true);
                }
            }

            mainCam = Camera.main;
        }

        private void OnEnable()
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }
        }

        public void Setup(
            int damage,
            DamageType damageType,
            bool isCrit,
            Color textColor,
            float fontSize,
            TMP_FontAsset fontAsset = null)
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TMP_Text>();
            }

            if (textMesh != null)
            {
                if (fontAsset != null)
                {
                    textMesh.font = fontAsset;
                }
                else if (textMesh.font == null)
                {
                    TMP_FontAsset fallbackFont = Resources.Load<TMP_FontAsset>("IbarraRealNova-VariableFont_wght SDF");
                    if (fallbackFont != null)
                    {
                        textMesh.font = fallbackFont;
                    }
                }

                textMesh.fontSize = fontSize;
                textMesh.color = textColor;
                baseColor = textColor;

                // Format tekstu - czysta czytelna cyfra bez wykrzykników
                if (damageType == DamageType.Heal)
                {
                    textMesh.text = $"+{damage}";
                }
                else
                {
                    textMesh.text = damage.ToString();
                }

                textMesh.alignment = TextAlignmentOptions.Center;
            }

            // Dyskretniejsze i zbalansowane parametry skali
            if (isCrit)
            {
                targetBaseScale = 1.05f;
                punchScaleMultiplier = 1.2f;
            }
            else if (damageType == DamageType.Poison || damageType == DamageType.Bleed)
            {
                targetBaseScale = 0.8f;
                punchScaleMultiplier = 1.05f;
            }
            else
            {
                targetBaseScale = 0.9f;
                punchScaleMultiplier = 1.15f;
            }

            // Losowy wektor bocznego rozrzutu (lewo/prawo i lekki przód/tył)
            float randomX = Random.Range(-horizontalSpread, horizontalSpread);
            float randomZ = Random.Range(-horizontalSpread * 0.5f, horizontalSpread * 0.5f);
            moveVelocity = new Vector3(randomX, floatSpeed, randomZ);

            transform.localScale = Vector3.one * (targetBaseScale * 0.5f);
            currentLifetime = lifetime;
        }

        private void Update()
        {
            if (currentLifetime <= 0f) return;

            float dt = Time.deltaTime;
            currentLifetime -= dt;
            float progress = 1f - (currentLifetime / lifetime); // od 0 do 1

            // 1. Ruch z łukiem i opadaniem prędkości
            transform.position += moveVelocity * dt;
            moveVelocity.y = Mathf.Max(0.2f, moveVelocity.y - deceleration * dt);
            moveVelocity.x *= (1f - dt * 2f);
            moveVelocity.z *= (1f - dt * 2f);

            // 2. Soczysta animacja skali (Punch Scale)
            float currentScale;
            if (progress < 0.2f)
            {
                // Wyskakiwanie i powiększanie (punch)
                float t = progress / 0.2f;
                currentScale = Mathf.Lerp(targetBaseScale * 0.5f, targetBaseScale * punchScaleMultiplier, t);
            }
            else if (progress < 0.4f)
            {
                // Miękkie opadanie do właściwego rozmiaru
                float t = (progress - 0.2f) / 0.2f;
                currentScale = Mathf.Lerp(targetBaseScale * punchScaleMultiplier, targetBaseScale, t);
            }
            else
            {
                // Delikatne zmniejszanie pod koniec
                float t = (progress - 0.4f) / 0.6f;
                currentScale = Mathf.Lerp(targetBaseScale, targetBaseScale * 0.8f, t);
            }
            transform.localScale = Vector3.one * currentScale;

            // 3. Płynny Fade Out (w drugiej połowie życia)
            if (progress > 0.5f && textMesh != null)
            {
                float fadeT = (progress - 0.5f) / 0.5f;
                Color c = baseColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                textMesh.color = c;
            }

            // 4. Koniec cyklu życia -> zwrot do puli
            if (currentLifetime <= 0f)
            {
                if (DamagePopupManager.Instance != null)
                {
                    DamagePopupManager.Instance.ReturnToPool(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        private void LateUpdate()
        {
            // Billboard: zawsze twarzą w stronę kamery bez odbić lustrzanych
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }

            if (mainCam != null)
            {
                transform.rotation = mainCam.transform.rotation;
            }
        }
    }
}
