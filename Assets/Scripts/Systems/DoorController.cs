using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentAnimationRoutine;

    public string InteractionPrompt => isOpen ? "Close Door" : "Open Door";

    private void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    public bool Interact(GameObject interactor)
    {
        // Allow interruption
        isOpen = !isOpen;

        if (currentAnimationRoutine != null)
        {
            StopCoroutine(currentAnimationRoutine);
        }

        currentAnimationRoutine = StartCoroutine(AnimateDoor(isOpen ? openRotation : closedRotation));
        return true;
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation;
        currentAnimationRoutine = null;
    }
}
