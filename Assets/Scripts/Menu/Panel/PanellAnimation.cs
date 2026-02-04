using UnityEngine;

public class InfoSettingsAnimation : MonoBehaviour
{
    public Animator panelAnimator;
    public string boolName = "Settings";  

    private bool isOpen = false;

    void Start()
    {
        panelAnimator.SetBool(boolName, false);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        panelAnimator.SetBool(boolName, isOpen);
    }
}