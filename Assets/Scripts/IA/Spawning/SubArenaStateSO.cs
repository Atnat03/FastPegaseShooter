using UnityEngine;

[CreateAssetMenu(fileName = "SubArenaStateSO", menuName = "Scriptable Objects/SubArenaStateSO")]
public class SubArenaStateSO : ScriptableObject
{
    public int p_budgetPerSecond;
    public Color p_color;
    public Sprite p_icon;
    public string p_name;
}
