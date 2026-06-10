using TMPro;
using UnityEngine;

public class AscensseurIncrement : Ascenseur
{
    public int currentIdx;
    public string baseTxt = "GR-0";
    public TextMeshPro tmp;
    
    protected override void OnLoop()
    {
        currentIdx++;
        tmp.text = baseTxt + currentIdx;
    }
}
