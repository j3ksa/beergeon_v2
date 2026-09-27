using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

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

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void SetCurrentHealth(int hp)
    {
        if (maxHealth <= 0 && playerStats != null) maxHealth = playerStats.GetMaxHealth();
        if (maxHealth <= 0) maxHealth = 100;
        currentHealth = Mathf.Clamp(hp, 1, maxHealth);
        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }

    private int bonusMaxHealth = 0;

    public void SetBonusMaxHealth(int bonus)
    {
        int diff = bonus - bonusMaxHealth;
        bonusMaxHealth = bonus;
        UpdateMaxHealth();
        if (diff > 0)
        {
            currentHealth += diff;
            currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
            OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
        }
    }

    private void UpdateMaxHealth()
    {
        int baseMax = playerStats != null ? playerStats.GetMaxHealth() : 100;
        int oldMax = maxHealth;
        maxHealth = baseMax + bonusMaxHealth;

        if (maxHealth > oldMax && oldMax > 0)
        {
            currentHealth += (maxHealth - oldMax);
        }
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }

    private void Update()
    {
        if (isDecaying && currentHealth > 0)
        {
            float currentDecay = decayRate;
            // 3x szybszy ubytek zdrowia przy Ślepym Szale
            if (Praktyki.SlepySzal.SlepySzalBuff.IsActive)
            {
                currentDecay *= Praktyki.SlepySzal.SlepySzalBuff.DecayMultiplier;
            }

            healthDecayAccumulator += currentDecay * Time.deltaTime;

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

        // Unik (I-Frames podczas dasha pod L-Ctrl)
        if (playerController != null && playerController.HasIFrames)
        {
            Debug.Log("<color=#00FFFF><b>[Unik]</b></color> Atak przeciwnika uniknięty (I-Frames dasha)!");
            return;
        }

        int finalDamage = damage;
        // Redukcja obrażeń z Ognistej Wody
        if (Praktyki.OgnistaWoda.OgnistaWodaBuff.IsActive && damage > 0)
        {
            float reduction = Praktyki.OgnistaWoda.OgnistaWodaBuff.DamageReductionPercent;
            finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * (1f - reduction)));
            Debug.Log($"<color=#B0C4DE><b>[Ognista Woda]</b></color> Zredukowano obrażenia z {damage} do {finalDamage} (-{reduction * 100:F0}%).");
        }

        // Redukcja obrażeń z pancerza (Minecraft Armor)
        if (Praktyki.EQ.MinecraftInventoryUI.Instance != null && Praktyki.EQ.MinecraftInventoryUI.Instance.TotalDefense > 0 && finalDamage > 0)
        {
            int def = Praktyki.EQ.MinecraftInventoryUI.Instance.TotalDefense;
            float armorReduction = Mathf.Clamp(def * 0.04f, 0f, 0.80f);
            finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * (1f - armorReduction)));
        }

        currentHealth -= finalDamage;
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
        Praktyki.SlepySzal.SlepySzalBuff.CureBuff();
        OnDeath?.Invoke();
        // Handle death logic here (disable controls, show game over, etc.)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
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
