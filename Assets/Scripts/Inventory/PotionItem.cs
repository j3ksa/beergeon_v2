using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Inventory/Potion")]
public class PotionItem : EquippableItem
{
    public enum PotionType { Health, Speed, Strength, Stamina }

    [Header("Potion Effects")]
    public PotionType potionType;
    public float amount; // Heal amount, stamina amount, or multiplier
    public float duration; // For speed/strength
    
    public override void Use(PlayerController player, PlayerHand hand)
    {
        // Example implementation - relies on Player components being accessible
        switch (potionType)
        {
            case PotionType.Health:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null) health.Heal((int)amount);
                break;
            case PotionType.Speed:
                Praktyki.SpeedPotion.PlayerSpeedBuff speedBuff = player.GetComponent<Praktyki.SpeedPotion.PlayerSpeedBuff>();
                if (speedBuff == null) speedBuff = player.gameObject.AddComponent<Praktyki.SpeedPotion.PlayerSpeedBuff>();
                speedBuff.ApplySpeedBuff(amount > 0 ? amount : 1.5f, duration > 0 ? duration : 10f);
                break;
            case PotionType.Strength:
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null) stats.ApplyDamageBuff(amount, duration);
                break;
            case PotionType.Stamina:
                PlayerStamina stamina = player.GetComponent<PlayerStamina>();
                if (stamina == null) stamina = Object.FindFirstObjectByType<PlayerStamina>();
                if (stamina != null)
                {
                    if (amount > 0)
                    {
                        stamina.RestoreStamina(amount);
                    }
                    else
                    {
                        stamina.RestoreStaminaToFull();
                    }
                }
                Praktyki.ByczePaliwo.ByczePaliwoUI.TriggerFlash();
                break;
            // Add other cases as needed
        }

        // Remove from inventory after use
        Inventory.instance.Remove(this);
    }
}
