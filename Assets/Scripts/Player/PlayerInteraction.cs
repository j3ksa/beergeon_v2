using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private Camera playerCamera;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    // Removed Update() direct key check as requested to use Action Reference exclusively

    // Removed OnInteract callback method as we are now polling in Update

    public void PerformInteraction()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        int layerMask = interactableLayers.value == 0 ? ~0 : interactableLayers.value;

        if (Physics.Raycast(ray, out hit, interactionRange, layerMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject);
            }
            else
            {
                // Check parent
                 interactable = hit.collider.GetComponentInParent<IInteractable>();
                 if (interactable != null)
                 {
                     interactable.Interact(gameObject);
                 }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionRange);
        }
    }
}
