using UnityEngine;

public interface IInteractable
{
    string InteractionPrompt { get; }
    bool Interact(GameObject interactor);
}
