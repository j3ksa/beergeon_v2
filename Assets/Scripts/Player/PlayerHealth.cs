using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float decayRate = 1f; // Health lost per second
    [SerializeField] private bool isDecaying = true;

    private float healthDecayAccumulator;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;

    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        if (playerStats != null)
        {
            maxHealth = playerStats.GetMaxHealth();
            // Listen for stats changes to update max health
            playerStats.OnStatsChanged.AddListener(UpdateMaxHealth);
        }

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
    }

    private void UpdateMaxHealth()
    {
        if (playerStats != null)
        {
            int oldMax = maxHealth;
            maxHealth = playerStats.GetMaxHealth();
            
            // Adjust current health proportionally or just clamp? 
            // Let's increase current health by the difference if max increased
            if (maxHealth > oldMax)
            {
                currentHealth += (maxHealth - oldMax);
            }
            
            OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
        }
    }

    private void Update()
    {
        if (isDecaying && currentHealth > 0)
        {
            healthDecayAccumulator += decayRate * Time.deltaTime;

            if (healthDecayAccumulator >= 1f)
            {
                int damageToTake = Mathf.FloorToInt(healthDecayAccumulator);
                TakeDamage(damageToTake);
                healthDecayAccumulator -= damageToTake;
            }
        }
    }

    private void Start()
    {
        // Initialize UI
        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return; // Can't heal if dead? Design choice.

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }

    private void Die()
    {
        
        OnDeath?.Invoke();
        // Handle death logic here (disable controls, show game over, etc.)
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
        }
    }
}
