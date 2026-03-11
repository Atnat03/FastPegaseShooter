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
        
        [SerializeField] public MeshRenderer _model;
        [SerializeField] public AudioSource _source;
        [SerializeField] public SoundsDataSO _soundData;
        [SerializeField] public VisualEffect _muzzleFlash; // test

        private bool ShootingInputPressed;

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
                   
            if (GetCurrentAmmo() > 0 && !_reloadModule.IsReloading)
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

                _recoilModule?.Recoil();
                SetAmmo(GetCurrentAmmo() - 1);
                _muzzleFlash?.Play();
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
                s.TryShoot();
                _muzzleFlash?.Play();
                _recoilModule?.Recoil();
                SetAmmo(GetCurrentAmmo() - 1);
                yield return new WaitForSeconds(s.FireRate);
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
            _isOverload.Value = isOverload;
            SurchargeMultiplierDamage = dmgMultiplicator;
            SurchargeMultiplierRate = cadenceMultiplicator;
            foreach (IShootModule s in _shootModule)
            {
                s?.AmmoModule.SetDamage(dmgMultiplicator);
            }
        }

        public void TryReload()
        {
            if (_reloadModule.IsReloading) return;
            
            _reloadModule?.Reload();

            PlayReloadSoundObserverRpc();
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
        private void PlayReloadSoundObserverRpc()
        {
            AudioClip clip = SoundManager.GetAudioClip(_soundData, "Reload");
            SoundManager.PlaySound(clip, _source);
        }

        public void StopReload() => _reloadModule.StopReload();

    }
}