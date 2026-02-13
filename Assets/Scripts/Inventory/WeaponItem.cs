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
        // Perform attack using hand's raycast logic for now
        hand.PerformRaycast();
        Debug.Log($"Used weapon: {itemName}");
    }
}
