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

    private void Awake()
    {
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
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
        Debug.Log("Player Died");
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
