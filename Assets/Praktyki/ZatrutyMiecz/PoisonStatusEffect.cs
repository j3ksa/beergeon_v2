using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Praktyki.ZatrutyMiecz
{
    /// <summary>
    /// Efekt statusu trucizny nakładany na przeciwnika.
    /// Zadaje cykliczne obrażenia (DoT), barwi przeciwnika na jadowity zielony kolor
    /// i odświeża czas trwania przy ponownym uderzeniu.
    /// </summary>
    public class PoisonStatusEffect : MonoBehaviour
    {
        [Header("Ustawienia Wizualne Trucizny")]
        [SerializeField] private Color poisonTint = new Color(0.35f, 1.0f, 0.35f, 1.0f); // Jadowita zieleń

        private IDamageable damageable;
        private EnemyHealth enemyHealth;
        private SpriteRenderer[] renderers;
        private Color[] originalColors;

        private float remainingDuration = 0f;
        private float tickInterval = 1f;
        private int damagePerTick = 3;
        private Coroutine poisonRoutine;
        private bool isPoisoned = false;

        public bool IsPoisoned => isPoisoned;
        public float RemainingDuration => remainingDuration;
        public int CurrentDamagePerTick => damagePerTick;

        private void Awake()
        {
            damageable = GetComponent<IDamageable>();
            if (damageable == null) damageable = GetComponentInParent<IDamageable>();

            enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth == null) enemyHealth = GetComponentInParent<EnemyHealth>();

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
        /// Nakłada lub odświeża efekt trucizny na przeciwniku.
        /// </summary>
        public void ApplyPoison(int damage, float duration, float interval = 1f)
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

            if (!isPoisoned)
            {
                isPoisoned = true;
                if (poisonRoutine != null) StopCoroutine(poisonRoutine);
                poisonRoutine = StartCoroutine(PoisonCoroutine());
                Debug.Log($"<color=#39FF14>[Zatruty Miecz]</color> Przeciwnik {gameObject.name} został ZATRUТY! ({damagePerTick} dmg co {tickInterval}s przez {remainingDuration}s)");
            }
            else
            {
                Debug.Log($"<color=#39FF14>[Zatruty Miecz]</color> Odświeżono czas zatrucia na przeciwniku {gameObject.name} (Pozostało: {remainingDuration}s)");
            }
        }

        private IEnumerator PoisonCoroutine()
        {
            float nextTickTime = Time.time + tickInterval;

            while (remainingDuration > 0f)
            {
                yield return null;

                // Sprawdź czy przeciwnik nadal żyje
                if (enemyHealth != null && enemyHealth.CurrentHealth <= 0)
                {
                    break;
                }

                remainingDuration -= Time.deltaTime;

                // Zadaj obrażenia przy każdym ticku
                if (Time.time >= nextTickTime && remainingDuration > 0f)
                {
                    nextTickTime = Time.time + tickInterval;

                    if (damageable != null)
                    {
                        DamageNumbers.DamagePopupContext.SetNext(DamageNumbers.DamageType.Poison);
                        damageable.TakeDamage(damagePerTick);
                        Debug.Log($"<color=#39FF14>[Zatruty Miecz - Tick]</color> {gameObject.name} otrzymał {damagePerTick} obrażeń od trucizny! (Pozostało: {remainingDuration:F1}s)");
                    }

                    // Jeśli po ciosie trucizny zginął, kończymy
                    if (enemyHealth != null && enemyHealth.CurrentHealth <= 0)
                    {
                        break;
                    }
                }
            }

            EndPoison();
        }

        private void LateUpdate()
        {
            if (!isPoisoned || renderers == null) return;

            // Barwimy na zielono tylko wtedy, gdy EnemyHealth nie flashuje na czerwono
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    // Jeśli renderer nie jest aktualnie zabarwiony na czerwono (np. przez uderzenie)
                    if (renderers[i].color != Color.red)
                    {
                        renderers[i].color = poisonTint;
                    }
                }
            }
        }

        public void EndPoison()
        {
            if (poisonRoutine != null)
            {
                StopCoroutine(poisonRoutine);
                poisonRoutine = null;
            }

            isPoisoned = false;
            remainingDuration = 0f;
            RestoreOriginalColors();
            Debug.Log($"<color=#39FF14>[Zatruty Miecz]</color> Trucizna zeszła z przeciwnika {gameObject.name}.");
        }

        private void RestoreOriginalColors()
        {
            if (renderers == null || originalColors == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && i < originalColors.Length)
                {
                    renderers[i].color = originalColors[i];
                }
            }
        }

        private void OnDisable()
        {
            RestoreOriginalColors();
            isPoisoned = false;
            if (poisonRoutine != null)
            {
                StopCoroutine(poisonRoutine);
                poisonRoutine = null;
            }
        }

        private void OnDestroy()
        {
            RestoreOriginalColors();
        }
    }
}
