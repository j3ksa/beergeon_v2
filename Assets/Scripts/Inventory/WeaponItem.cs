using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponItem : EquippableItem
{
    [Header("Weapon Stats")]
    public int damage = 10;
    public float range = 2.0f;
    [Tooltip("Optional: specific sprite for attack frame. If null, hand may just rotate/punch.")]
    public Sprite attackSprite;

    public override void Use(PlayerController player, PlayerHand hand)
    {
        // Delegate attack logic back to PlayerHand or perform here
        // For now, PlayerHand has the raycast logic, so we might want to expose a method there or move it to PlayerInteraction?
        // To keep it clean, let's keep physical attack logic in components but define stats here.
        
    }
}
