using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int strength = 10;
    [SerializeField] private int vitality = 10;
    [SerializeField] private int experience = 0;
    [SerializeField] private int level = 1;

    [Header("Config")]
    [SerializeField] private int baseXpRequirement = 100;
    [SerializeField] private float xpMultiplier = 1.5f;

    public UnityEvent OnLevelUp;
    public UnityEvent OnStatsChanged;

    private static int savedStrength = 10;
    private static int savedVitality = 10;
    private static int savedExperience = 0;
    private static int savedLevel = 1;
    private static bool isInitialized = false;

    public static void ResetStats()
    {
        isInitialized = false;
    }

    public void SetStats(int newLevel, int newExperience, int newStrength, int newVitality)
    {
        level = newLevel;
        experience = newExperience;
        strength = newStrength;
        vitality = newVitality;

        savedLevel = level;
        savedExperience = experience;
        savedStrength = strength;
        savedVitality = vitality;
        isInitialized = true;

        OnStatsChanged?.Invoke();
    }

    private void Awake()
    {
        if (!isInitialized)
        {
            savedStrength = strength;
            savedVitality = vitality;
            savedExperience = experience;
            savedLevel = level;
            isInitialized = true;
        }
        else
        {
            strength = savedStrength;
            vitality = savedVitality;
            experience = savedExperience;
            level = savedLevel;
        }
    }

    private float damageMultiplier = 1.0f;
    private float buffTimer = 0f;

    private void Update()
    {
        if (buffTimer > 0)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0)
            {
                damageMultiplier = 1.0f;
                Debug.Log("[PlayerStats] Buff obrażeń wygasł.");
            }
        }
    }

    private int bonusStrength = 0;

    public void SetBonusStrength(int bonus)
    {
        bonusStrength = bonus;
        OnStatsChanged?.Invoke();
    }

    public int Strength => strength + bonusStrength;
    public int Vitality => vitality;
    public int Experience => experience;
    public int Level => level;
    public float CurrentDamageMultiplier => damageMultiplier;
    public bool IsBuffActive => buffTimer > 0;

    public void ApplyDamageBuff(float multiplier, float duration)
    {
        damageMultiplier = multiplier;
        buffTimer = duration;
        Debug.Log($"[PlayerStats] Aktywowano buff obrażeń x{multiplier} na {duration} sekund!");
    }

    public int GetDamage(int baseDamage)
    {
        // Formula: (Base Damage + (Strength + bonusStrength)) * damageMultiplier
        float calculatedDamage = (baseDamage + (strength + bonusStrength)) * damageMultiplier;

        // Mnożnik ze Ślepego Szału (+200% = x3.0)
        if (Praktyki.SlepySzal.SlepySzalBuff.IsActive)
        {
            calculatedDamage *= Praktyki.SlepySzal.SlepySzalBuff.DamageMultiplier;
        }

        return Mathf.RoundToInt(calculatedDamage);
    }

    public int GetMaxHealth()
    {
        // Simple formula: 1 Vitality = 10 Health
        return vitality * 10;
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        CheckLevelUp();
        savedExperience = experience;
        OnStatsChanged?.Invoke();
    }

    private void CheckLevelUp()
    {
        int requiredXp = GetXpForNextLevel();
        while (experience >= requiredXp)
        {
            experience -= requiredXp;
            level++;

            savedLevel = level;
            savedStrength = strength;
            savedVitality = vitality;

            OnLevelUp?.Invoke();
            OnStatsChanged?.Invoke();
            requiredXp = GetXpForNextLevel();
        }
    }

    public int GetXpForNextLevel()
    {
        return Mathf.FloorToInt(baseXpRequirement * Mathf.Pow(xpMultiplier, level - 1));
    }

    /// <summary>
    /// Bezpośredni awans gracza o zadaną liczbę poziomów (np. po wypiciu Pitnej Wiedzy),
    /// z zachowaniem obecnego postępu XP do kolejnego poziomu.
    /// </summary>
    public void LevelUpDirectly(int levelsToAdd = 1)
    {
        if (levelsToAdd <= 0) return;

        for (int i = 0; i < levelsToAdd; i++)
        {
            level++;
            savedLevel = level;
            savedStrength = strength;
            savedVitality = vitality;

            OnLevelUp?.Invoke();
        }

        savedExperience = experience;
        OnStatsChanged?.Invoke();
        Debug.Log($"<color=#FFD700><b>[PlayerStats]</b></color> Awans bezpośredni o +{levelsToAdd} lvl! Aktualny poziom: {level}.");
    }
}
