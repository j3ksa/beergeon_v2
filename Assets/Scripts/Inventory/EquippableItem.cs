using UnityEngine;

public abstract class EquippableItem : Item
{
    [Header("Visuals")]
    public Sprite heldSprite; // Sprite shown in hand when idle

    [Header("Stats")]
    [Tooltip("Time between uses")]
    public float cooldown = 0.5f;

    public abstract void Use(PlayerController player, PlayerHand hand);
}
