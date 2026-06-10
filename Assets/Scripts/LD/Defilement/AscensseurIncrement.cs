using TMPro;
using UnityEngine;

public class AscensseurIncrement : Ascenseur
{
    [SerializeField]private int currentIdx;
    [SerializeField]private string baseTxt = "GR-0";
    [SerializeField]private TextMeshPro tmp;
    [SerializeField]private Animator animator;
    [SerializeField]private bool isRight;
    
    protected override void OnLoop()
    {
        currentIdx++;
        tmp.text = baseTxt + currentIdx;
    }

    protected override void OnAscenseurStop(float duration)
    { 
        if (isRight)
        {
            if (elapsed / duration < .5 && elapsed / duration > .45)
            {
                animator.SetTrigger("Open");
            }
        }
    }
}
