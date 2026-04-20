using System;
using System.Collections.Generic;
using GunDecorator;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Gun/Gun Module Settings")]
public class GunModuleSettingsSO : ScriptableObject
{
    [SerializeReference]
    public List<GunSetting> modulesList = new();
}

[Serializable]
public class GunSetting
{
    [HideInInspector] public string displayName;
    [HideInInspector] public Color headerColor;
}

#region Child Settings Class

    //TemplateShoot
    public class TemplateShootSetting : GunSetting
    {
        public float fireRate;
        public float numberBulletSpread;
        [Range(0, 30)]public float SpreadAngle;
    }

    //RaycastAmmo
    public class RaycastAmmoSetting : GunSetting
    {
        public float maxDistance = 2000;
        public float damages;
        public float bulletSpeed = 100;
    }

    //PhysicAmmo
    public class PhysicAmmoSetting : GunSetting
    {
        public float damages;
        public float mass;
        public float bulletThrowForce = 100;
        public float bulletSpeed = 50;
    }


    //Reload
    public class ReloadSetting : GunSetting
    {
        public bool isAutoReload;
        public int magazineSize;
        public float reloadDuration;
    }

    //Recoil
    public class RecoilSetting : GunSetting
    {
        [Header("Settings")]
        public AnimationCurve recoilXCurve= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public float recoilX = 5;
        public float recoilY = 0.25f;
        public float recoilZ = 0.35f;
        public float returnSpeed = 6;
        public float snapiness = 15f;
        
        [Header("Z Kickback")]
        public float z_recoilDistance = 0.15f;
        public float z_returnSpeed = 8f;
        public float maxZKickback = 0.15f;
    }

    //SECOND

    //Explosif
    public class S_ExplosifSetting : GunSetting
    {
        public float explosionRadius;
    }

    //Noise
    public class S_NoiseSetting : GunSetting
    {
        public float MaxOffsetX;
        public float MaxOffsetY;
    }

    //Salve
    public class S_SalveSetting : GunSetting
    {
        public int numberShootPerSalve;
        public float intervalDuration;
    }


    //Charged Salve
    public class ChargedSalveSetting : GunSetting
    {
        public bool IsExplosifAmmo = false;
        public float explosionRadius;

        [Header("Charging")] 
        public float timeToCharge = 1;
        public float DeadZoneStartCharging = 0.5f;
        public float recoilChargedMultiplier = 1.25f;
        public float RecoilX = 2;
        public float IsFullMultiplicator = 0.9f;
        public int NumberBulletInCharged = 10;

        [Header("Salve")]
        public float intervaleCharge = 0.05f;
    }

    //Charged Increase Noise
    public class ChargedIncreaseNoiseSetting : GunSetting
    {
        public bool IsExplosifAmmo = false;
        public float explosionRadius;

        [Header("Charging")] 
        public float timeToCharge = 1;
        public float DeadZoneStartCharging = 0.5f;
        public float recoilChargedMultiplier = 1.25f;
        public float RecoilX = 2;
        public float IsFullMultiplicator = 0.9f;
        public int NumberBulletInCharged = 10;

        [Header("Noise")] 
        public AnimationCurve NoiseEvolutionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public float maxNoiseAngle = 10;
    }

    //Charged Decrease Noise
    public class ChargedDecreaseNoiseSetting : GunSetting
    {
        public bool IsExplosifAmmo = false;
        public float explosionRadius;

        [Header("Charging")] 
        public float timeToCharge = 1;
        public float DeadZoneStartCharging = 0.5f;
        public float recoilChargedMultiplier = 1.25f;
        public float RecoilX = 2;
        public float IsFullMultiplicator = 0.9f;
        public int NumberBulletInCharged = 10;

        [Header("Noise")] 
        public AnimationCurve NoiseEvolutionCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        public float startMaxNoiseAngle = 10;
    }

#endregion