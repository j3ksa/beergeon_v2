using System.Collections;
using UnityEngine;

namespace Praktyki.StaryHmielnik
{
    /// <summary>
    /// Efekt zatrucia gracza po trafieniu szyszką chmielu (15% szansy).
    /// Zadaje 10 HP obrażeń na sekundę przez 5 sekund oraz barwi pasek życia na zielono.
    /// </summary>
    [DisallowMultipleComponent]
    public class HopPoisonDebuff : MonoBehaviour
    {
        private PlayerHealth playerHealth;
        private IDamageable damageable;

        private float remainingDuration = 0f;
        private float tickInterval = 1.0f;
        private int damagePerTick = 10;
        private Coroutine poisonCoroutine;
        private bool isPoisonActive = false;

        public bool IsActive => isPoisonActive;
        public float RemainingTime => remainingDuration;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            damageable = GetComponent<IDamageable>();
        }

        /// <summary>
        /// Aplikuje lub odświeża efekt zatrucia.
        /// </summary>
        /// <param name="duration">Czas trwania (np. 5 sekund)</param>
        /// <param name="dps">Obrażenia na sekundę (np. 10 HP)</param>
        public void ApplyPoison(float duration = 5f, int dps = 10)
        {
            // Ochrona przed zatruciem dzięki Mleku Starego
            if (Praktyki.MlekoStarego.MlekoStaregoBuff.IsImmune)
            {
                Debug.Log("[HopPoisonDebuff] Gracz jest odporny na truciznę dzięki Mleku Starego!");
                return;
            }

            damagePerTick = dps;
            remainingDuration = Mathf.Max(remainingDuration, duration);

            if (!isPoisonActive)
            {
                if (poisonCoroutine != null)
                {
                    StopCoroutine(poisonCoroutine);
                }
                poisonCoroutine = StartCoroutine(PoisonRoutine());
            }

            Debug.Log($"[HopPoisonDebuff] Gracz zatruty! Traci {damagePerTick} HP/s przez {remainingDuration}s. Pasek życia staje się zielony.");
        }

        private IEnumerator PoisonRoutine()
        {
            isPoisonActive = true;

            // Zmiana koloru paska życia na zielony
            HealthBar.SetPoisonState(true);

            float tickTimer = 0f;

            while (remainingDuration > 0f)
            {
                remainingDuration -= Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= tickInterval)
                {
                    tickTimer -= tickInterval;
                    DealPoisonDamage();
                }

                yield return null;
            }

            RemovePoison();
        }

        private void DealPoisonDamage()
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerTick);
            }
            else if (damageable != null)
            {
                damageable.TakeDamage(damagePerTick);
            }
        }

        public void RemovePoison()
        {
            if (poisonCoroutine != null)
            {
                StopCoroutine(poisonCoroutine);
                poisonCoroutine = null;
            }

            if (isPoisonActive)
            {
                isPoisonActive = false;
                // Przywrócenie pierwotnego (czerwonego) koloru paska życia
                HealthBar.SetPoisonState(false);
                Debug.Log("[HopPoisonDebuff] Zatrucie wygasło. Pasek życia przywrócony do normalnego koloru.");
            }

            remainingDuration = 0f;
        }

        private void OnDisable()
        {
            RemovePoison();
        }

        private void OnDestroy()
        {
            RemovePoison();
        }
    }
}
