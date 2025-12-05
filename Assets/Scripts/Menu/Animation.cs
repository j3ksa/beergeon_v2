using UnityEngine;

public class AutoSlidePanel : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        animator.Play("Settings"); 
    }
}