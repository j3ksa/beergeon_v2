using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;

    [Header("Drops")]
    [SerializeField] private GameObject healthPotionPrefab;
    [SerializeField] private float dropChance = 0.3f;
    [SerializeField] private int xpReward = 25;

    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private float flashEndTime;
    private bool isFlashing = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        
        if (renderers != null && renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].color;
            }
        }
    }

    private void LateUpdate()
    {
        if (renderers == null || renderers.Length == 0) return;

        // Check if we are currently in the flash duration
        if (Time.time < flashEndTime)
        {
            isFlashing = true;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = Color.red;
            }
        }
        else if (isFlashing)
        {
            // We were flashing, but time is up. Restore colors once.
            isFlashing = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = originalColors[i];
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {currentHealth}");
        OnDamageTaken?.Invoke(damage);

        // Trigger Flash
        flashEndTime = Time.time + 0.1f;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        OnDeath?.Invoke();
        
        // Drop Chance
        // Create explicit block for clarity
        if (healthPotionPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healthPotionPrefab, transform.position, Quaternion.identity);
        }

        // Grant XP
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.GainExperience(xpReward);
        }

        // Simple death handling: Destroy object
        // ideally we might play an animation or spawn a corpse
        Destroy(gameObject);
    }
}
