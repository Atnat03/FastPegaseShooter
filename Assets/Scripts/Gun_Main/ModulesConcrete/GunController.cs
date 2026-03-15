using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.VFX;

public interface IGun
{
    public void TryFire();
    public void TryCancelShooting();
    public void TryReload();
    public void TriggerHitMark(bool isCritique = false);
}


public interface ISurcharge
{
    public int GetCurrentAmmo();
    public void SetAmmo(int value);
    public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator);
    public bool IsOverload { get; }
    public MeshRenderer ModelGun { get; }
    public void StopReload();
}

namespace GunDecorator
{
    public class GunController : NetworkBehaviour, IGun, ISurcharge
    {
        public bool IsOverload => _isOverload.Value;
        public float SurchargeMultiplierDamage { get; set; }
        public float SurchargeMultiplierRate { get; set; }
        
        public IRecoilModule RecoilModule => _recoilModule;
        
        public MeshRenderer ModelGun => _model;

        private IShootModule[] _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;
        private IHitMarkerModule _hitMarkerModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);
        
        [SerializeField, Tooltip("Model 3d de l'arme")] 
        public MeshRenderer _model;
        [SerializeField, Tooltip("Audio Source de l'arme")] 
        public AudioSource _source;
        [SerializeField, Tooltip("Scriptable Object contenant les Audio Clip de l'arme (exemple dans le dossier Assets/SoudData)")] 
        public SoundsDataSO _soundData;
        [SerializeField, Tooltip("Effet de tir du bout du canon de l'arme")] public VisualEffect _muzzleFlash; // test

        private bool ShootingInputPressed;

        [HideInInspector] public bool p_authorizedToShoot = true;

        private void Awake()
        {
            //On récupere tout les types de modules possible et potentiellement sur l'arme
            _shootModule = GetComponents<IShootModule>();
            _reloadModule = GetComponent<IReloadModule>();
            _recoilModule = GetComponent<IRecoilModule>();
            _hitMarkerModule = GetComponent<IHitMarkerModule>();

            //On initialise tout les modules de l'arme
            foreach (GunModule module in GetComponents<GunModule>())
            {
                module.Initialize(this);
            }
        }

        public void TryFire()
        {
            //On appele la fonction shoot du module de shoot actuellement équipé

            ShootingInputPressed = true;
                   
            if (GetCurrentAmmo() > 0 && !_reloadModule.IsReloading && p_authorizedToShoot)
            {
                foreach (IShootModule s in _shootModule)
                {
                    if (s is { IsFullAuto: true })
                    {
                        StartCoroutine(ShootingCoroutine(s));
                        continue;
                    }
                    s?.TryShoot();
                }

                _recoilModule?.Recoil(_model.transform, 0.1f);
                SetAmmo(GetCurrentAmmo() - 1);
                PlayMuzzleFlash();
            }

            if (GetCurrentAmmo() <= 0)
            {
                if (_reloadModule.AutoReload)
                {
                    TryReload();
                }
            }
        }


        IEnumerator ShootingCoroutine(IShootModule s)
        {
            while (ShootingInputPressed && GetCurrentAmmo() > 0 && !_reloadModule.IsReloading)
            {
                p_authorizedToShoot = false;
                s.TryShoot();
                PlayMuzzleFlash();
                _recoilModule?.Recoil(_model.transform, s.FireRate);
                SetAmmo(GetCurrentAmmo() - 1);
                
                yield return new WaitForSeconds(s.FireRate);
                
                p_authorizedToShoot =  true;
            }
        }
        

        public void TryCancelShooting()
        {
            ShootingInputPressed = false;
            foreach (IShootModule s in _shootModule)
            {
                s?.CancelShooting();
            }
        }

        public int GetCurrentAmmo() => _reloadModule.CurrentAmmo;

        public void SetAmmo(int value) => _reloadModule.SetAmmo(value);

        public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator)
        {
            if (!IsServerInitialized)
            {
                SetSurchargeStatServerRpc(isOverload, dmgMultiplicator, cadenceMultiplicator);
                return;
            }
            _isOverload.Value = isOverload;
            SurchargeMultiplierDamage = dmgMultiplicator;
            SurchargeMultiplierRate = cadenceMultiplicator;
            foreach (IShootModule s in _shootModule)
                s?.AmmoModule.SetDamage(dmgMultiplicator);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetSurchargeStatServerRpc(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator)
        {
            SetSurchargeStat(isOverload, dmgMultiplicator, cadenceMultiplicator);
        }

        public void TryReload()
        {
            if (_reloadModule.IsReloading) return;
            
            _reloadModule?.Reload();

            PlaySound("Reload");
        }

        public void TriggerHitMark(bool isCritique = false)
        {
            if (!isCritique)
            {
                _hitMarkerModule?.HitMark();
            }
            else
            {
                _hitMarkerModule?.HitMarkCritique();
            }
        }

        [ObserversRpc]
        public void PlaySound(string sound, float volume = 0.5f)
        {
            AudioClip clip = SoundManager.GetAudioClip(_soundData, sound);
            SoundManager.PlaySound(clip, _source, volume);
        }
        
                
        [ObserversRpc]
        private void PlayMuzzleFlash()
        {
            Debug.Log("Play Muzzle flash");
            _muzzleFlash.Play();
        }

        public void StopReload() => _reloadModule.StopReload();

    }
}