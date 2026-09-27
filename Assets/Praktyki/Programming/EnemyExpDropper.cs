using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyExpDropper : MonoBehaviour
{
    [Header("XP Drop Settings")]
    [SerializeField] private GameObject xpPrefab;
    [Tooltip("Czy ilość dropionego XP ma być losowana z zakresu min/max?")]
    [SerializeField] private bool useRandomXpRange = true;
    [SerializeField] private int minTotalXp = 10;
    [SerializeField] private int maxTotalXp = 20;
    [Tooltip("Wartość domyślna lub używana gdy useRandomXpRange = false")]
    [SerializeField] private int totalXpAmount = 15;
    [SerializeField] private int minPrefabsToSpawn = 1;
    [SerializeField] private int maxPrefabsToSpawn = 3;
    [SerializeField] private float spawnSpread = 0.5f;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath.AddListener(DropXP);
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath.RemoveListener(DropXP);
        }
    }

    private void DropXP()
    {
        if (xpPrefab == null)
        {
            Debug.LogWarning($"XP Prefab is not assigned on {gameObject.name}");
            return;
        }

        int actualTotalXp = useRandomXpRange 
            ? Random.Range(minTotalXp, maxTotalXp + 1) 
            : totalXpAmount;

        if (actualTotalXp <= 0) return;

        int spawnCount = Random.Range(minPrefabsToSpawn, maxPrefabsToSpawn + 1);
        if (spawnCount <= 0) return;

        // Distribute the total XP amount among the spawned items
        int xpPerItem = actualTotalXp / spawnCount;
        int remainingXp = actualTotalXp % spawnCount;

        for (int i = 0; i < spawnCount; i++)
        {
            // Add a small random offset to spread the spawned objects slightly
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnSpread, spawnSpread),
                0f, // Keeping y/z appropriate for 3D/2.5D space
                Random.Range(-spawnSpread, spawnSpread)
            );

            Vector3 spawnPosition = transform.position + randomOffset;
            GameObject spawnedXp = Instantiate(xpPrefab, spawnPosition, Quaternion.identity);

            // Assign the divided XP value to the ExpItem script
            ExpItem expItem = spawnedXp.GetComponent<ExpItem>();
            if (expItem != null)
            {
                int currentXpValue = xpPerItem + (i == 0 ? remainingXp : 0);
                expItem.SetXpValue(currentXpValue);
            }
        }
    }
}
