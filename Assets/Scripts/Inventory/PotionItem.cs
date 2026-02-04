using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Inventory/Potion")]
public class PotionItem : EquippableItem
{
    public enum PotionType { Health, Speed, Strength }

    [Header("Potion Effects")]
    public PotionType potionType;
    public float amount; // Heal amount or multiplier
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
            // Add other cases as needed
        }

        // Remove from inventory after use
        Inventory.instance.Remove(this);
    }
}
