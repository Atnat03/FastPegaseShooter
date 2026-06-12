using TMPro;
using UnityEngine;

public class AscensseurIncrement : Ascenseur
{
    [SerializeField]private int currentIdx;
    [SerializeField]private string baseTxt = "GR-0";
    [SerializeField]private TextMeshPro tmp;
    [SerializeField]private Animator animator;
    
    protected override void OnLoop()
    {
        currentIdx++;
        tmp.text = baseTxt + currentIdx;
    }

    protected override void OnAscenseurStop(float duration)
    { 
        base.OnAscenseurStop(duration);
        
        if (animator)
        {
            animator.SetTrigger("Open");
        }
    }
}
