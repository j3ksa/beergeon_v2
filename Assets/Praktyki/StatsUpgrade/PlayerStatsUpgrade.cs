using UnityEngine;
using UnityEngine.Events;

public class PlayerStatsUpgrade : MonoBehaviour
{
    public enum UpgradeMode
    {
        Percentage,  // Wzrost procentowy skumulowany / progresywny
        FlatValues   // Stały wzrost wartości
    }

    [Header("Składniki Gracza (Auto-find if empty)")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private PlayerController playerController;

    [Header("System Punktów i Skalowanie z Poziomem")]
    [SerializeField] private int availablePoints = 0;
    [Tooltip("Bazowa liczba punktów przyznawana na 1 poziomie (np. 1 pkt)")]
    [SerializeField] private int basePointsPerLevel = 1;
    [Tooltip("Czy wyższy poziom przyznaje dodatkowe punkty?")]
    [SerializeField] private bool scalePointsWithLevel = true;
    [Tooltip("Co ile poziomów gracz dostaje 1 dodatkowy punkt na poziom? (np. co 5 poziomów: Lvl 1-5 = 1pkt, Lvl 6-10 = 2pkt)")]
    [SerializeField] private int bonusPointInterval = 5;

    [Header("Tryb Wzrostu Statystyk")]
    [SerializeField] private UpgradeMode upgradeMode = UpgradeMode.Percentage;

    [Header("Wzrost Procentowy Progresywny za 1 Punkt (%)")]
    [SerializeField] private float healthPercentPerPoint = 8f;        // +8% HP
    [SerializeField] private float staminaPercentPerPoint = 6f;       // +6% Staminy
    [SerializeField] private float staminaRegenPercentPerPoint = 4f;   // +4% Regen (zmienia wartość wizualną przy KAŻDYM kliknięciu)
    [SerializeField] private float speedPercentPerPoint = 2f;         // +2% Prędkości
    [SerializeField] private float strengthPercentPerPoint = 12f;     // +12% Siły (odczuwalny przyrost!)
    [SerializeField] private float critChancePerPoint = 2.5f;        // +2.5% Krytyka na poziom

    [Header("Poziomy Ulepszeń")]
    [SerializeField] private int healthUpgrades = 0;
    [SerializeField] private int staminaUpgrades = 0;
    [SerializeField] private int staminaRegenUpgrades = 0;
    [SerializeField] private int speedUpgrades = 0;
    [SerializeField] private int strengthUpgrades = 0;
    [SerializeField] private int critChanceUpgrades = 0;

    [Header("Maksymalne Poziomy Ulepszeń (-1 = brak limitu)")]
    [SerializeField] private int maxHealthUpgrades = 20;
    [SerializeField] private int maxStaminaUpgrades = 20;
    [SerializeField] private int maxStaminaRegenUpgrades = 10;
    [SerializeField] private int maxSpeedUpgrades = 10;
    [SerializeField] private int maxStrengthUpgrades = 20;
    [SerializeField] private int maxCritChanceUpgrades = 15; // Max 37.5% krytyka

    [Header("Opcje Testowe (Dev / Testing)")]
    [SerializeField] private bool enableTestingKeys = true;
    [SerializeField] private UnityEngine.InputSystem.Key spawnXpKey = UnityEngine.InputSystem.Key.X;

    public UnityEvent OnPointsOrStatsChanged;

    public int AvailablePoints => availablePoints;
    public UpgradeMode CurrentUpgradeMode => upgradeMode;

    public int HealthUpgrades => healthUpgrades;
    public int StaminaUpgrades => staminaUpgrades;
    public int StaminaRegenUpgrades => staminaRegenUpgrades;
    public int SpeedUpgrades => speedUpgrades;
    public int StrengthUpgrades => strengthUpgrades;
    public int CritChanceUpgrades => critChanceUpgrades;

    // Bazowe wartości
    private int baseHealth = 100;
    private float baseStamina = 30f;
    private float baseStaminaRegen = 8f;
    private float baseMoveSpeed = 5f;
    private int baseStrength = 10;

    // Statyczne dane zachowujące ulepszenia między scenami/poziomami (Level_1 -> Level_2)
    private static int savedAvailablePoints = 0;
    private static int savedHealthUpgrades = 0;
    private static int savedStaminaUpgrades = 0;
    private static int savedStaminaRegenUpgrades = 0;
    private static int savedSpeedUpgrades = 0;
    private static int savedStrengthUpgrades = 0;
    private static int savedCritChanceUpgrades = 0;
    private static bool isStaticInitialized = false;

    public static void ResetUpgrades()
    {
        isStaticInitialized = false;
        savedAvailablePoints = 0;
        savedHealthUpgrades = 0;
        savedStaminaUpgrades = 0;
        savedStaminaRegenUpgrades = 0;
        savedSpeedUpgrades = 0;
        savedStrengthUpgrades = 0;
        savedCritChanceUpgrades = 0;
    }

    private void SaveStaticState()
    {
        savedAvailablePoints = availablePoints;
        savedHealthUpgrades = healthUpgrades;
        savedStaminaUpgrades = staminaUpgrades;
        savedStaminaRegenUpgrades = staminaRegenUpgrades;
        savedSpeedUpgrades = speedUpgrades;
        savedStrengthUpgrades = strengthUpgrades;
        savedCritChanceUpgrades = critChanceUpgrades;
        isStaticInitialized = true;
    }

    private void Awake()
    {
        FindPlayerComponents();

        if (isStaticInitialized)
        {
            availablePoints = savedAvailablePoints;
            healthUpgrades = savedHealthUpgrades;
            staminaUpgrades = savedStaminaUpgrades;
            staminaRegenUpgrades = savedStaminaRegenUpgrades;
            speedUpgrades = savedSpeedUpgrades;
            strengthUpgrades = savedStrengthUpgrades;
            critChanceUpgrades = savedCritChanceUpgrades;
        }
        else
        {
            SaveStaticState();
        }
    }

    private void FindPlayerComponents()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (playerStamina == null) playerStamina = GetComponent<PlayerStamina>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        FindPlayerComponents();

        if (playerStats != null)
        {
            playerStats.OnLevelUp.AddListener(OnPlayerLevelUp);
            baseStrength = playerStats.Strength;
        }

        if (playerHealth != null)
        {
            baseHealth = playerStats != null ? playerStats.GetMaxHealth() : 100;
        }

        if (playerStamina != null)
        {
            baseStamina = playerStamina.MaxStamina;
            baseStaminaRegen = playerStamina.EffectiveStaminaRegen;
        }

        if (playerController != null)
        {
            baseMoveSpeed = playerController.MoveSpeed;
        }

        ApplyAllUpgrades();
        SaveStaticState();
    }

    private void Update()
    {
        if (enableTestingKeys && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current[spawnXpKey].wasPressedThisFrame)
        {
            if (playerStats == null) FindPlayerComponents();
            if (playerStats != null)
            {
                playerStats.GainExperience(100);
                Debug.Log("[Testing] Dodano 100 XP bezpośrednio graczo-postaci!");
            }
        }
    }

    public int CalculatePointsForLevel(int playerLevel)
    {
        int points = basePointsPerLevel;
        if (scalePointsWithLevel && bonusPointInterval > 0)
        {
            int bonus = (playerLevel - 1) / bonusPointInterval;
            points += bonus;
        }
        return points;
    }

    private void OnPlayerLevelUp()
    {
        int currentLevel = playerStats != null ? playerStats.Level : 1;
        int pointsGained = CalculatePointsForLevel(currentLevel);
        availablePoints += pointsGained;

        Debug.Log($"[StatsUpgrade] Awans na Poziom {currentLevel}! Otrzymano {pointsGained} pkt statystyk. Dostępne: {availablePoints}");
        OnPointsOrStatsChanged?.Invoke();
    }

    public void AddPoints(int amount)
    {
        availablePoints += amount;
        OnPointsOrStatsChanged?.Invoke();
    }

    #region Obliczanie Wartości Ulepszeń

    public int GetCurrentBaseHealth()
    {
        int cur = playerStats != null ? playerStats.GetMaxHealth() : baseHealth;
        return cur > 0 ? cur : 100;
    }

    public int GetBonusHealth()
    {
        int curBase = GetCurrentBaseHealth();
        if (healthUpgrades <= 0) return 0;
        float multiplier = Mathf.Pow(1f + (healthPercentPerPoint / 100f), healthUpgrades) - 1f;
        return Mathf.RoundToInt(curBase * multiplier);
    }

    public float GetBonusStamina()
    {
        float curBase = baseStamina > 0 ? baseStamina : 30f;
        if (staminaUpgrades <= 0) return 0f;
        float multiplier = Mathf.Pow(1f + (staminaPercentPerPoint / 100f), staminaUpgrades) - 1f;
        return curBase * multiplier;
    }

    public float GetBonusStaminaRegen()
    {
        float curBase = baseStaminaRegen > 0 ? baseStaminaRegen : 8f;
        if (staminaRegenUpgrades <= 0) return 0f;
        float multiplier = Mathf.Pow(1f + (staminaRegenPercentPerPoint / 100f), staminaRegenUpgrades) - 1f;
        return curBase * multiplier;
    }

    public float GetBonusSpeed()
    {
        float curBase = baseMoveSpeed > 0 ? baseMoveSpeed : 5f;
        if (speedUpgrades <= 0) return 0f;
        float multiplier = Mathf.Pow(1f + (speedPercentPerPoint / 100f), speedUpgrades) - 1f;
        return curBase * multiplier;
    }

    public int GetBonusStrength()
    {
        int curBase = baseStrength > 0 ? baseStrength : 10;
        if (strengthUpgrades <= 0) return 0;
        float multiplier = Mathf.Pow(1f + (strengthPercentPerPoint / 100f), strengthUpgrades) - 1f;
        int bonus = Mathf.RoundToInt(curBase * multiplier);
        // Gwarantujemy co najmniej +2 Siły za każde ulepszenie, by atak rosnął bardzo odczuwalnie!
        return Mathf.Max(strengthUpgrades * 2, bonus);
    }

    public float GetCurrentCritChance()
    {
        return critChanceUpgrades * critChancePerPoint;
    }

    #endregion

    #region Upgrade Actions

    public bool UpgradeHealth()
    {
        if (!CanUpgradeHealth()) return false;
        availablePoints--;
        healthUpgrades++;
        ApplyHealthUpgrade();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
        return true;
    }

    public bool UpgradeStamina()
    {
        if (!CanUpgradeStamina()) return false;
        availablePoints--;
        staminaUpgrades++;
        ApplyStaminaUpgrade();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
        return true;
    }

    public bool UpgradeStaminaRegen()
    {
        if (!CanUpgradeStaminaRegen()) return false;
        availablePoints--;
        staminaRegenUpgrades++;
        ApplyStaminaRegenUpgrade();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
        return true;
    }

    public bool UpgradeSpeed()
    {
        if (!CanUpgradeSpeed()) return false;
        availablePoints--;
        speedUpgrades++;
        ApplySpeedUpgrade();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
        return true;
    }

    public bool UpgradeStrength()
    {
        if (!CanUpgradeStrength()) return false;
        availablePoints--;
        strengthUpgrades++;
        ApplyStrengthUpgrade();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
        return true;
    }

    public bool UpgradeCritChance()
    {
        if (!CanUpgradeCritChance()) return false;
        availablePoints--;
        critChanceUpgrades++;
        ApplyCritChanceUpgrade();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
        return true;
    }

    #endregion

    #region Can Upgrade Checks

    public bool CanUpgradeHealth() => CanUpgrade(healthUpgrades, maxHealthUpgrades);
    public bool CanUpgradeStamina() => CanUpgrade(staminaUpgrades, maxStaminaUpgrades);
    public bool CanUpgradeStaminaRegen() => CanUpgrade(staminaRegenUpgrades, maxStaminaRegenUpgrades);
    public bool CanUpgradeSpeed() => CanUpgrade(speedUpgrades, maxSpeedUpgrades);
    public bool CanUpgradeStrength() => CanUpgrade(strengthUpgrades, maxStrengthUpgrades);
    public bool CanUpgradeCritChance() => CanUpgrade(critChanceUpgrades, maxCritChanceUpgrades);

    private bool CanUpgrade(int currentLevel, int maxLevel)
    {
        if (availablePoints <= 0) return false;
        if (maxLevel >= 0 && currentLevel >= maxLevel) return false;
        return true;
    }

    #endregion

    #region Apply Upgrades

    public void ApplyAllUpgrades()
    {
        ApplyHealthUpgrade();
        ApplyStaminaUpgrade();
        ApplyStaminaRegenUpgrade();
        ApplySpeedUpgrade();
        ApplyStrengthUpgrade();
        ApplyCritChanceUpgrade();
    }

    private void ApplyHealthUpgrade()
    {
        if (playerHealth != null)
        {
            playerHealth.SetBonusMaxHealth(GetBonusHealth());
        }
    }

    private void ApplyStaminaUpgrade()
    {
        if (playerStamina != null)
        {
            playerStamina.SetBonusMaxStamina(GetBonusStamina());
        }
    }

    private void ApplyStaminaRegenUpgrade()
    {
        if (playerStamina != null)
        {
            playerStamina.SetBonusStaminaRegen(GetBonusStaminaRegen());
        }
    }

    private void ApplySpeedUpgrade()
    {
        if (playerController != null)
        {
            playerController.MoveSpeed = baseMoveSpeed + GetBonusSpeed();
        }
    }

    private void ApplyStrengthUpgrade()
    {
        if (playerStats != null)
        {
            playerStats.SetBonusStrength(GetBonusStrength());
        }
    }

    private void ApplyCritChanceUpgrade()
    {
        // Crit Chance przeliczane dynamicznie przy obliczaniu obrażeń
    }

    /// <summary>
    /// Oblicza ostateczne obrażenia z uwzględnieniem szansy na trafienie krytyczne.
    /// </summary>
    public int CalculateFinalDamage(int baseDamage)
    {
        int damage = playerStats != null ? playerStats.GetDamage(baseDamage) : baseDamage;

        float randomVal = Random.Range(0f, 100f);
        if (randomVal <= GetCurrentCritChance())
        {
            damage = Mathf.RoundToInt(damage * 1.35f); // 135% obrażeń przy krytyku (zbalansowane)
            Debug.Log($"[StatsUpgrade] TRAFIENIE KRYTYCZNE! Obrażenia: {damage}");
        }

        return damage;
    }

    #endregion

    #region Save / Load Data Support

    public void LoadUpgradeState(int points, int hpLvl, int staminaLvl, int staminaRegenLvl, int speedLvl, int strengthLvl, int critLvl)
    {
        availablePoints = points;
        healthUpgrades = hpLvl;
        staminaUpgrades = staminaLvl;
        staminaRegenUpgrades = staminaRegenLvl;
        speedUpgrades = speedLvl;
        strengthUpgrades = strengthLvl;
        critChanceUpgrades = critLvl;

        ApplyAllUpgrades();
        SaveStaticState();
        OnPointsOrStatsChanged?.Invoke();
    }

    #endregion
}
