using UnityEngine;

public class HidePanelInfo : MonoBehaviour
{
    public Animator panelAnimator;
    public string BoolName;  

    public void Hide()
    {
        panelAnimator.SetBool(BoolName, false);
    }
    public void Show()
    {
        panelAnimator.SetBool(BoolName, true);
    }
}