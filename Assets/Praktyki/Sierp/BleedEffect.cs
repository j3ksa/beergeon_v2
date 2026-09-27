using System.Collections;
using UnityEngine;

namespace Praktyki.Sierp
{
    /// <summary>
    /// Komponent nakładany na przeciwnika zranionego 3. ciosem Sierpa.
    /// Powoduje periodyczne krwawienie (DoT), które może zostać zerwane przez naładowany PPM Sierpa.
    /// </summary>
    public class BleedEffect : MonoBehaviour
    {
        private float remainingDuration = 0f;
        private int damagePerTick = 4;
        private float tickInterval = 1f;
        private float nextTickTime = 0f;
        private IDamageable damageable;

        public bool IsBleeding => remainingDuration > 0f;
        public float RemainingDuration => remainingDuration;
        public int DamagePerTick => damagePerTick;

        private void Awake()
        {
            damageable = GetComponent<IDamageable>() ?? GetComponentInParent<IDamageable>();
        }

        public void ApplyBleed(float duration, int dmgPerTick)
        {
            remainingDuration = Mathf.Max(remainingDuration, duration);
            damagePerTick = dmgPerTick;
            nextTickTime = Time.time + tickInterval;
        }

        private void Update()
        {
            if (remainingDuration <= 0f) return;

            remainingDuration -= Time.deltaTime;

            if (Time.time >= nextTickTime && remainingDuration > 0f)
            {
                nextTickTime = Time.time + tickInterval;
                if (damageable != null)
                {
                    DamageNumbers.DamagePopupContext.SetNext(DamageNumbers.DamageType.Bleed);
                    damageable.TakeDamage(damagePerTick);
                    Debug.Log($"<color=#DC143C>[Krwawienie]</color> {gameObject.name} otrzymał {damagePerTick} obrażeń od krwawienia.");
                }
            }
        }

        /// <summary>
        /// Zrywa krwawienie (Rupture). Zwraca pozostałą liczbę tików do obliczenia bonusowych obrażeń.
        /// </summary>
        public int ConsumeBleed()
        {
            int remainingTicks = Mathf.Max(1, Mathf.CeilToInt(remainingDuration / tickInterval));
            remainingDuration = 0f;
            return remainingTicks;
        }
    }
}
