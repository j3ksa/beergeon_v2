using UnityEngine;

public class settingsAnimation : MonoBehaviour
{
    public Animator panelAnimator;
    public string boolName = "Settings";

    public bool isOpen = false;

    void Awake()
    {
        if (panelAnimator == null) panelAnimator = GetComponent<Animator>();
    }

    private bool CanAnimate()
    {
        return panelAnimator != null && panelAnimator.runtimeAnimatorController != null && panelAnimator.isActiveAndEnabled;
    }

    void OnEnable()
    {
        // Sync animator with current state when enabled
        if (CanAnimate())
        {
            try { panelAnimator.SetBool(boolName, isOpen); }
            catch (System.Exception) { }
        }
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        if (isOpen) Open();
        else Close();
    }

    public void Open()
    {
        isOpen = true;
        if (CanAnimate())
        {
            try { panelAnimator.SetBool(boolName, true); }
            catch (System.Exception) { Debug.LogWarning($"[settingsAnimation] Animator Parameter '{boolName}' missing! Please add a Bool parameter named '{boolName}' to your Animator."); }
        }
    }

    public void Close()
    {
        isOpen = false;
        if (CanAnimate())
        {
            try { panelAnimator.SetBool(boolName, false); }
            catch (System.Exception) { /* Already warned in Open or Start usually */ }
        }
    }
}