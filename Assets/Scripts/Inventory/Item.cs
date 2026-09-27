using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null;
    public GameObject pickupPrefab;
    [TextArea] public string description = "Item description";

    [Header("Stack Settings")]
    public bool isStackable = false;
    public int maxStack = 64;
}
