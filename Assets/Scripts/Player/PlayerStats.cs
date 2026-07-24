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

    public int Strength => strength;
    public int Vitality => vitality;
    public int Experience => experience;
    public int Level => level;

    public int GetDamage(int baseDamage)
    {
        // Formula: Base Damage + (Strength * 0.1)
        return Mathf.RoundToInt(baseDamage + (strength * 0.1f));
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
            strength += 1; // Stat gain per level
            vitality += 1; // Stat gain per level

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
}
