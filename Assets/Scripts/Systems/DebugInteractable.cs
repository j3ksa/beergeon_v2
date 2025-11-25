using UnityEngine;

public class DebugInteractable : MonoBehaviour, ICollidable, ITriggerable
{
    public void OnCollide(GameObject source)
    {
        Debug.Log($"[DebugInteractable] Collided with {source.name}");
    }

    public void OnTrigger(GameObject source)
    {
        Debug.Log($"[DebugInteractable] Triggered by {source.name}");
    }
}
