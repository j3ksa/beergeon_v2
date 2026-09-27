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
    [Tooltip("Mnożnik prędkości powrotu broni do pozycji spoczynkowej (np. 0.5 = 2x dłuższy powrót)")]
    public float returnRotationSpeedMultiplier = 1.0f;

    [Header("Position & Scale")]
    public Vector3 handPositionOffset = Vector3.zero;
    public float handScaleMultiplier = 1.0f;

    public abstract void Use(PlayerController player, PlayerHand hand);
    public virtual void AltUse(PlayerController player, PlayerHand hand) {}

    /// <summary>
    /// Wywoływane gdy przedmiot zostaje wyciągnięty do ręki gracza.
    /// </summary>
    public virtual void OnEquipped(PlayerController player, PlayerHand hand) {}

    /// <summary>
    /// Wywoływane gdy gracz chowa przedmiot z ręki lub zmienia na inny.
    /// </summary>
    public virtual void OnUnequipped(PlayerController player, PlayerHand hand) {}
}
