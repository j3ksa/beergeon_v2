using UnityEngine;

namespace Praktyki.ZatrutyMiecz
{
    [CreateAssetMenu(fileName = "ZatrutyMiecz", menuName = "Inventory/Weapons/Zatruty Miecz")]
    public class ZatrutyMieczItem : WeaponItem
    {
        [Header("Statystyki Trucizny (Poison Settings)")]
        [Range(0f, 1f)]
        [Tooltip("Szansa na zatrucie celu przy uderzeniu (np. 0.35 = 35%)")]
        public float poisonChance = 0.35f;

        [Tooltip("Czas trwania trucizny w sekundach")]
        public float poisonDuration = 5.0f;

        [Tooltip("Interwał zadawania obrażeń przez truciznę (w sekundach)")]
        public float poisonTickInterval = 1.0f;

        [Tooltip("Ilość obrażeń zadawanych przy każdym ticku trucizny")]
        public int poisonDamagePerTick = 3;

        private void Reset()
        {
            itemName = "Zatruty Miecz";
            damage = 40; // 35 (kij bejsbolowy) + 5
            range = 4.0f;
            cooldown = 0.4f;
            useItemRotation = true;
            rotationEuler = new Vector3(90, -80, -40);
            rotationSpeed = 10f;
            handPositionOffset = new Vector3(-0.05f, 0.45f, 0f);
            handScaleMultiplier = 1.1f;
            description = "Miecz nasycony jadowitą trucizną. Zadaje 40 obrażeń bezpośrednich i posiada 35% szansy na zatrucie wroga (3 pkt obrażeń co 1s przez 5s).";
        }

        public override void OnHit(GameObject target, IDamageable damageable, RaycastHit hit)
        {
            base.OnHit(target, damageable, hit);

            if (damageable == null && target == null) return;

            // Obliczenie parametrów trucizny z uwzględnieniem poziomu kowadła (+5% szansy, +1 DMG/s na poziom)
            int upgradeLvl = 0;
            if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
            {
                upgradeLvl = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(this);
            }

            float effectivePoisonChance = Mathf.Clamp01(poisonChance + (upgradeLvl * 0.05f));
            int effectivePoisonDmg = poisonDamagePerTick + upgradeLvl;

            // Sprawdź szansę na zatrucie
            if (Random.value <= effectivePoisonChance)
            {
                MonoBehaviour mb = damageable as MonoBehaviour;
                GameObject enemyObj = mb != null ? mb.gameObject : target;

                if (enemyObj != null && enemyObj.activeInHierarchy)
                {
                    EnemyHealth health = enemyObj.GetComponent<EnemyHealth>();
                    if (health == null) health = enemyObj.GetComponentInParent<EnemyHealth>();
                    if (health != null && health.CurrentHealth <= 0) return;

                    PoisonStatusEffect statusEffect = enemyObj.GetComponent<PoisonStatusEffect>();
                    if (statusEffect == null)
                    {
                        statusEffect = enemyObj.AddComponent<PoisonStatusEffect>();
                    }

                    statusEffect.ApplyPoison(effectivePoisonDmg, poisonDuration, poisonTickInterval);
                    Debug.Log($"<color=#7CFC00>[Zatruty Miecz]</color> Nałożono truciznę na {enemyObj.name}! Obrażenia: {effectivePoisonDmg}/s (Szansa: {effectivePoisonChance * 100:0}%, Poziom: +{upgradeLvl})");
                }
            }
        }
    }
}
