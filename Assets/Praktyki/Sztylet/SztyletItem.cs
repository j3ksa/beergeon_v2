using UnityEngine;
using Praktyki.KowadloUlepszen;

namespace Praktyki.Sztylet
{
    [CreateAssetMenu(fileName = "Sztylet", menuName = "Inventory/Weapons/Sztylet")]
    public class SztyletItem : WeaponItem
    {
        [Header("Statystyki Zwinności Sztyletu")]
        [Tooltip("Bonus do prędkości ruchu gracza gdy sztylet jest w ręku (0.25 = +25%)")]
        [Range(0f, 1f)]
        public float moveSpeedBonus = 0.25f;

        [Tooltip("Mnożnik zużycia staminy podczas sprintu (0.9 = 10% wolniejsze zużycie)")]
        [Range(0.1f, 1f)]
        public float staminaDrainMultiplier = 0.90f;

        [Header("Trafienie Krytyczne")]
        [Tooltip("Szansa na trafienie krytyczne (0.10 = 10%)")]
        [Range(0f, 1f)]
        public float critChance = 0.10f;

        [Tooltip("Mnożnik obrażeń krytycznych (np. 2.0 = 200%)")]
        public float critMultiplier = 2.0f;

        [Header("Efekt Krwawienia")]
        [Tooltip("Szansa na nałożenie krwawienia przy trafieniu (0.05 = 5%)")]
        [Range(0f, 1f)]
        public float bleedChance = 0.05f;

        [Tooltip("Czas trwania krwawienia w sekundach")]
        public float bleedDuration = 3.0f;

        [Tooltip("Interwał ticków krwawienia")]
        public float bleedTickInterval = 0.5f;

        [Tooltip("Obrażenia zadawane przy każdym ticku krwawienia")]
        public int bleedDamagePerTick = 1;

        [Header("System Serii Ciosów (Combo)")]
        [Tooltip("Maksymalny czas między ciosami podtrzymujący serię combo")]
        public float comboResetWindow = 1.2f;

        [Tooltip("Maksymalny stopień combo (np. 5)")]
        public int maxComboCount = 5;

        // Stan tymczasowy
        private int currentCombo = 0;
        private float lastHitTime = 0f;

        private void Reset()
        {
            itemName = "Sztylet";
            damage = 12; // ok. 1/3 bejsbola (35 dmg)
            range = 2.2f; // krótki, zwarty zasięg sztyletu (wymaga podejścia blisko)
            cooldown = 0.24f; // o 40% szybszy niż bejsbol (0.40s -> 0.24s)
            useItemRotation = true;
            rotationEuler = new Vector3(80, -60, -35);
            rotationSpeed = 16f; // bardzo szybki powrót i zamach
            returnRotationSpeedMultiplier = 1.4f;
            handPositionOffset = new Vector3(0.04f, 0.10f, 0.05f);
            handScaleMultiplier = 0.80f;
            description = "Zwinny nóż sprężynowy. Zadaje 12 obrażeń, atakuje o 40% szybciej, zwiększa prędkość ruchu o 25% i zużywa staminę o 10% wolniej. Posiada 10% szansy na trafienie krytyczne (2x dmg) oraz 10% szansy na krwawienie.";
        }

        public override void OnEquipped(PlayerController player, PlayerHand hand)
        {
            base.OnEquipped(player, hand);

            if (player != null)
            {
                // Zwiększamy prędkość ruchu o 25%
                player.WeaponSpeedModifier = 1f + moveSpeedBonus;

                // Zmniejszamy tempo zużycia staminy o 10%
                PlayerStamina stamina = player.GetComponent<PlayerStamina>();
                if (stamina != null)
                {
                    stamina.StaminaDrainMultiplier = staminaDrainMultiplier;
                }

                Debug.Log($"<color=#FFD700>[Sztylet]</color> Wyciągnięto sztylet: +{moveSpeedBonus * 100}% do prędkości ruchu, staminę zużywasz o {Mathf.RoundToInt((1f - staminaDrainMultiplier) * 100)}% wolniej!");
            }
        }

        public override void OnUnequipped(PlayerController player, PlayerHand hand)
        {
            base.OnUnequipped(player, hand);

            if (player != null)
            {
                // Przywracamy bazowe wartości
                player.WeaponSpeedModifier = 1f;

                PlayerStamina stamina = player.GetComponent<PlayerStamina>();
                if (stamina != null)
                {
                    stamina.StaminaDrainMultiplier = 1f;
                }

                Debug.Log("<color=#FFD700>[Sztylet]</color> Schowano sztylet: przywrócono standardową prędkość i zużycie staminy.");
            }

            currentCombo = 0;
        }

        public override void OnHit(GameObject target, IDamageable damageable, RaycastHit hit)
        {
            base.OnHit(target, damageable, hit);

            if (damageable == null && target == null) return;

            // 1. Obsługa Combo
            if (Time.time - lastHitTime <= comboResetWindow)
            {
                currentCombo = Mathf.Min(maxComboCount, currentCombo + 1);
            }
            else
            {
                currentCombo = 1;
            }
            lastHitTime = Time.time;

            int upgradeLvl = 0;
            int bonusDamage = 0;
            if (WeaponUpgradeManager.Instance != null)
            {
                upgradeLvl = WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
                bonusDamage = WeaponUpgradeManager.Instance.GetBonusDamage(this);
            }

            // 2. Szansa na Trafienie Krytyczne (10% + 2% za poziom ulepszenia) + bonus mistrzowski (+15%)
            float totalCritChance = critChance + (upgradeLvl * 0.02f);
            if (upgradeLvl >= 6)
            {
                totalCritChance += 0.15f; // Bonus mistrzowski kowadła
            }

            int baseWeaponDamage = damage + bonusDamage;
            bool isCrit = Random.value <= totalCritChance;
            if (isCrit)
            {
                int bonusCritDamage = Mathf.RoundToInt(baseWeaponDamage * (critMultiplier - 1f));
                DamageNumbers.DamagePopupContext.SetNext(DamageNumbers.DamageType.Critical, isCrit: true);
                damageable.TakeDamage(bonusCritDamage);
                Debug.Log($"<color=#FF3333><b>[SZTYLET - KRYTYK!]</b></color> Zadano dodatkowe {bonusCritDamage} obrażeń krytycznych! (Combo: {currentCombo}x, Szansa kryta: {totalCritChance * 100:0}%)");
            }
            else
            {
                Debug.Log($"<color=#FFD700>[Sztylet - Trafienie]</color> Cios w cel (Combo: {currentCombo}x)");
            }

            // 3. Szansa na Krwawienie (skaluje się z kowadłem: +3% szansy, +0.5s czasu trwania)
            float effectiveBleedChance = Mathf.Clamp01(bleedChance + (upgradeLvl * 0.03f));
            float effectiveBleedDuration = bleedDuration + (upgradeLvl * 0.5f);
            int effectiveBleedDmg = bleedDamagePerTick + (upgradeLvl >= 3 ? 1 : 0);

            if (Random.value <= effectiveBleedChance)
            {
                MonoBehaviour mb = damageable as MonoBehaviour;
                GameObject enemyObj = mb != null ? mb.gameObject : target;

                if (enemyObj != null && enemyObj.activeInHierarchy)
                {
                    EnemyHealth health = enemyObj.GetComponent<EnemyHealth>() ?? enemyObj.GetComponentInParent<EnemyHealth>();
                    if (health != null && health.CurrentHealth <= 0) return;

                    BleedStatusEffect bleedEffect = enemyObj.GetComponent<BleedStatusEffect>();
                    if (bleedEffect == null)
                    {
                        bleedEffect = enemyObj.AddComponent<BleedStatusEffect>();
                    }

                    bleedEffect.ApplyBleed(effectiveBleedDmg, effectiveBleedDuration, bleedTickInterval);
                    Debug.Log($"<color=#FF1493>[Sztylet]</color> Nałożono krwawienie na {enemyObj.name}! Czas: {effectiveBleedDuration:0.0}s, Obrażenia: {effectiveBleedDmg}/tick (Szansa: {effectiveBleedChance * 100:0}%)");
                }
            }
        }
    }
}
