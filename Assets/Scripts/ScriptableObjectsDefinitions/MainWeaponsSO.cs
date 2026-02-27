using System;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "mainWeapons", menuName = "Weapons/new main Weapon")]
public class MainWeaponsSO : ScriptableObject
{
    public GameObject p_weaponVisual;
    public float p_recoilOffsetIntensity;
    public float p_recoilOffsetCompensation;
    public float p_recoilTorkIntensity;
    public float p_recoilTorkCompensation;
}
