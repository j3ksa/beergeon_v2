using UnityEngine;

namespace CurrencySystem
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyCapDropper : MonoBehaviour
    {
        [Header("Caps Drop Settings")]
        [SerializeField] private GameObject capPrefab;
        [Tooltip("Czy ilość dropionych kapsli ma być losowana z zakresu min/max?")]
        [SerializeField] private bool useRandomCapRange = true;
        [SerializeField] private int minTotalCaps = 3;
        [SerializeField] private int maxTotalCaps = 7;
        [Tooltip("Wartość domyślna lub używana gdy useRandomCapRange = false")]
        [SerializeField] private int totalCapAmount = 5;
        [SerializeField] private int minPrefabsToSpawn = 1;
        [SerializeField] private int maxPrefabsToSpawn = 3;
        [SerializeField] private float spawnSpread = 0.5f;

        private EnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.OnDeath.AddListener(DropCaps);
            }
        }

        private void OnDestroy()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnDeath.RemoveListener(DropCaps);
            }
        }

        private void DropCaps()
        {
            if (capPrefab == null)
            {
                Debug.LogWarning($"[EnemyCapDropper] Cap Prefab nie został przypisany na obiekcie {gameObject.name}");
                return;
            }

            int actualTotalCaps = useRandomCapRange 
                ? Random.Range(minTotalCaps, maxTotalCaps + 1) 
                : totalCapAmount;

            if (actualTotalCaps <= 0) return;

            int spawnCount = Random.Range(minPrefabsToSpawn, maxPrefabsToSpawn + 1);
            if (spawnCount <= 0) return;

            int capsPerItem = actualTotalCaps / spawnCount;
            int remainingCaps = actualTotalCaps % spawnCount;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnSpread, spawnSpread),
                    0f,
                    Random.Range(-spawnSpread, spawnSpread)
                );

                Vector3 spawnPosition = transform.position + randomOffset;
                GameObject spawnedCap = Instantiate(capPrefab, spawnPosition, Quaternion.identity);

                CapItem capItem = spawnedCap.GetComponent<CapItem>();
                if (capItem != null)
                {
                    int currentCapValue = capsPerItem + (i == 0 ? remainingCaps : 0);
                    capItem.SetCapValue(currentCapValue);
                }
            }
        }
    }
}
