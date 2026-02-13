using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour, IInteractable
{
    [Header("Portal Settings")]
    [Tooltip("The name of the scene to load when interacted with.")]
    [SerializeField] private string sceneToLoad;

    // Return empty string as user requested no prompt
    public string InteractionPrompt => "";

    public bool Interact(GameObject interactor)
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            return true;
        }
        else
        {
            Debug.LogWarning("PortalController: No scene specified to load!");
            return false;
        }
    }
}
