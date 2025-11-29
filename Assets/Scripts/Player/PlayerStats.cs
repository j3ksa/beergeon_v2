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

    public int Strength => strength;
    public int Vitality => vitality;
    public int Experience => experience;
    public int Level => level;

    public int GetDamage()
    {
        // Simple formula: 1 Strength = 1 Base Damage
        return strength;
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
    }

    private void CheckLevelUp()
    {
        int requiredXp = GetXpForNextLevel();
        while (experience >= requiredXp)
        {
            experience -= requiredXp;
            level++;
            strength += 2; // Stat gain per level
            vitality += 2; // Stat gain per level
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
