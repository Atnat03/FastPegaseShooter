using UnityEngine;

[CreateAssetMenu(fileName = "mainWeapons", menuName = "Weapons/new main Weapon")]
public class MainWeaponsSO : ScriptableObject
{
    public GameObject p_weaponVisual;
    public float p_recoilOffsetIntensity;
    public float p_recoilTorkIntensity;
}
