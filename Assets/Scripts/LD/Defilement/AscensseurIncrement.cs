using TMPro;
using UnityEngine;

public class AscensseurIncrement : Ascenseur
{
    [SerializeField]private int currentIdx;
    [SerializeField]private string baseTxt = "GR-0";
    [SerializeField]private TextMeshPro tmp;
    
    protected override void OnLoop()
    {
        currentIdx++;
        tmp.text = baseTxt + currentIdx;
    }
}
