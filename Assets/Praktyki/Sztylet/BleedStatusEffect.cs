using System.Collections;
using UnityEngine;

namespace Praktyki.Sztylet
{
    /// <summary>
    /// Efekt statusu krwawienia nakładany na przeciwnika po trafieniu sztyletem.
    /// Zadaje cykliczne obrażenia DoT, barwi sprite'a przeciwnika na krwistoczerwony odcień
    /// i przywraca oryginalne barwy po zakończeniu trwania.
    /// </summary>
    public class BleedStatusEffect : MonoBehaviour
    {
        [Header("Wizualia Krwawienia")]
        [Tooltip("Stała barwa krwawiącego przeciwnika")]
        [SerializeField] private Color bleedTint = new Color(1.0f, 0.45f, 0.45f, 1.0f); // Wyraźny różowo-czerwony odcień krwawienia
        [Tooltip("Intensywny błysk przy każdym tiku obrażeń")]
        [SerializeField] private Color tickFlashTint = new Color(0.85f, 0.0f, 0.0f, 1.0f); // Głęboka, krwista czerwień przy uderzeniu

        private IDamageable damageable;
        private EnemyHealth enemyHealth;
        private SpriteRenderer[] renderers;
        private Color[] originalColors;

        private float remainingDuration = 0f;
        private float tickInterval = 0.5f;
        private int damagePerTick = 1;
        private Coroutine bleedRoutine;
        private bool isBleeding = false;
        private float tickFlashEndTime = 0f;

        public bool IsBleeding => isBleeding;
        public float RemainingDuration => remainingDuration;

        private void Awake()
        {
            damageable = GetComponent<IDamageable>() ?? GetComponentInParent<IDamageable>();
            enemyHealth = GetComponent<EnemyHealth>() ?? GetComponentInParent<EnemyHealth>();
            CacheRenderers();
        }

        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            if (renderers != null && renderers.Length > 0)
            {
                originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        originalColors[i] = renderers[i].color;
                    }
                }
            }
        }

        /// <summary>
        /// Aplikuje lub odświeża krwawienie na celu.
        /// </summary>
        public void ApplyBleed(int damage, float duration, float interval = 0.5f)
        {
            if (!gameObject.activeInHierarchy) return;
            if (enemyHealth != null && enemyHealth.CurrentHealth <= 0) return;

            damagePerTick = damage;
            remainingDuration = duration;
            tickInterval = Mathf.Max(0.1f, interval);

            if (renderers == null || renderers.Length == 0)
            {
                CacheRenderers();
            }

            if (!isBleeding)
            {
                isBleeding = true;
                if (bleedRoutine != null) StopCoroutine(bleedRoutine);
                bleedRoutine = StartCoroutine(BleedCoroutine());
                Debug.Log($"<color=#FF2222>[Sztylet]</color> Przeciwnik {gameObject.name} KRWAWI! ({damagePerTick} dmg co {tickInterval}s przez {remainingDuration}s)");
            }
            else
            {
                Debug.Log($"<color=#FF2222>[Sztylet]</color> Odświeżono krwawienie na przeciwniku {gameObject.name} (Pozostało: {remainingDuration:F1}s)");
            }
        }

        private IEnumerator BleedCoroutine()
        {
            float nextTickTime = Time.time + tickInterval;

            while (remainingDuration > 0f)
            {
                yield return null;

                if (enemyHealth != null && enemyHealth.CurrentHealth <= 0)
                {
                    break;
                }

                remainingDuration -= Time.deltaTime;

                if (Time.time >= nextTickTime && remainingDuration > 0f)
                {
                    nextTickTime = Time.time + tickInterval;

                    // Pulsujący błysk mocnej czerwieni przy uderzeniu tiku krwawienia
                    tickFlashEndTime = Time.time + 0.15f;

                    if (damageable != null)
                    {
                        DamageNumbers.DamagePopupContext.SetNext(DamageNumbers.DamageType.Bleed);
                        damageable.TakeDamage(damagePerTick);
                        Debug.Log($"<color=#FF2222>[Sztylet - Krwawienie]</color> {gameObject.name} traci {damagePerTick} HP z krwawienia! (Zostało: {remainingDuration:F1}s)");
                    }

                    if (enemyHealth != null && enemyHealth.CurrentHealth <= 0)
                    {
                        break;
                    }
                }
            }

            EndBleed();
        }

        private void LateUpdate()
        {
            if (!isBleeding || renderers == null) return;

            // Wybieramy kolor: mocny ciemnoczerwony podczas tiku obrażeń, lub stały czerwony w trakcie trwania
            Color currentColor = (Time.time < tickFlashEndTime) ? tickFlashTint : bleedTint;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = currentColor;
                }
            }
        }

        private void EndBleed()
        {
            isBleeding = false;
            remainingDuration = 0f;

            if (bleedRoutine != null)
            {
                StopCoroutine(bleedRoutine);
                bleedRoutine = null;
            }

            RestoreOriginalColors();
        }

        private void RestoreOriginalColors()
        {
            if (renderers != null && originalColors != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && i < originalColors.Length)
                    {
                        renderers[i].color = originalColors[i];
                    }
                }
            }
        }

        private void OnDisable()
        {
            EndBleed();
        }

        private void OnDestroy()
        {
            EndBleed();
        }
    }
}
