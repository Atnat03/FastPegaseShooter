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

[System.Serializable]
public class GunSetting
{
    [HideInInspector] public string displayName;
    [HideInInspector] public Color headerColor;
}

#region Child Settings Class

    //TemplateShoot
    [System.Serializable]
    public class TemplateShootSetting : GunSetting
    {
        public float fireRate;
        public float numberBulletSpread;
        [Range(0, 30)]public float SpreadAngle;
        [Range(0, 0.5f)] public float RadiusOffset = 0f;
    }

    //RaycastAmmo
    [System.Serializable]
    public class RaycastAmmoSetting : GunSetting
    {
        public float maxDistance = 2000;
        public float damages;
        public float bulletSpeed = 100;
        public bool isDistanceReduced = false;
        public float factorReduceDamageByDistance = 1;
    }

    //PhysicAmmo
    [System.Serializable]
    public class PhysicAmmoSetting : GunSetting
    {
        public float damages;
        public float mass;
        public float bulletThrowForce = 100;
        public float bulletSpeed = 50;
    }


    //Reload
    [System.Serializable]
    public class ReloadSetting : GunSetting
    {
        public bool isAutoReload;
        public int magazineSize;
        public float reloadDuration;
    }

    //Recoil
    [System.Serializable]
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
    [System.Serializable]
    public class S_ExplosifSetting : GunSetting
    {
        public float explosionRadius;
    }

    //Noise
    [System.Serializable]
    public class S_NoiseSetting : GunSetting
    {
        public float MaxOffsetX = 2;
        public float MaxOffsetY = 2;
        public float TimeToAccessMaxNoise = 1;
        public AnimationCurve CurveNoiseOverTime = new AnimationCurve(new Keyframe(0,  0), new Keyframe(1, 1));
    }

    //Salve
    [System.Serializable]
    public class S_SalveSetting : GunSetting
    {
        public int numberShootPerSalve;
        public float intervalDuration;
        [Range(0, 30)] public float noiseCharged = 5;
    }


    //Charged Salve
    [System.Serializable]
    public class ChargedSalveSetting : GunSetting
    {
        public bool IsExplosifAmmo = false;
        public float explosionRadius;
        public Vector2 OneAmmoAddPercentage = new Vector2(1, 1);

        [Header("Charging")] 
        public float _damageChargedMultiplicator = 10;
        public float recoilChargedMultiplier = 1.25f;
        public float RecoilX = 2;
        public int NumberBulletInCharged = 10;

        [Header("Salve")]
        public float intervaleCharge = 0.05f;
        public Vector2 noiseCharged = new Vector2(0, 0);
        public int numberSalve = 1;
        public float intervaleBetweenSalve = 0.5f;
    }

    //Charged Increase Noise
    [System.Serializable]
    public class ChargedIncreaseNoiseSetting : GunSetting
    {
        public bool IsExplosifAmmo = false;
        public float explosionRadius;        
        public Vector2 OneAmmoAddPercentage = new Vector2(1, 1);
        
        [Header("Charging")]
        public float _damageChargedMultiplicator = 10;
        public float recoilChargedMultiplier = 1.25f;
        public float RecoilX = 2;
        public int NumberBulletInCharged = 10;

        [Header("Noise")] 
        public float noiseAngle = 5;
        public float maxNoiseAngle = 10;
    }

    //Charged Decrease Noise
    [System.Serializable]
    public class ChargedDecreaseNoiseSetting : GunSetting
    {
        public bool IsExplosifAmmo = false;
        public float explosionRadius;
        public Vector2 OneAmmoAddPercentage = new Vector2(1, 1);

        [Header("Charging")] 
        public float _damageChargedMultiplicator = 2;
        public float recoilChargedMultiplier = 1.25f;
        public float RecoilX = 2;
        public int NumberBulletInCharged = 10;

        [Header("Positions")] 
        public Vector3[] _posOffset;
    }

#endregion