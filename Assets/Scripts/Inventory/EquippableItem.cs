using UnityEngine;

public abstract class EquippableItem : Item
{
    [Header("Visuals")]
    public Sprite heldSprite; // Sprite shown in hand when idle

    [Header("Stats")]
    [Tooltip("Time between uses")]
    public float cooldown = 0.5f;

    [Header("Animation Settings")]
    public bool useItemRotation = false;
    public Vector3 rotationEuler = new Vector3(0, 0, -45);
    public float rotationSpeed = 10f;

    public abstract void Use(PlayerController player, PlayerHand hand);
}
