using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator.ChargedModules;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.VFX;

public interface IGun
{
    public void TryFire();
    public void TryCancelShooting();
    public void TryReload();
    public void TryCharging();
    public void TryShootCharged();

    public void Disable(bool state);
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
        private ChargedParentModule _chargedModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);
        
        [SerializeField, Tooltip("Model 3d de l'arme")] 
        public MeshRenderer _model;
        [SerializeField, Tooltip("Audio Source de l'arme")] 
        public AudioSource _source;
        [SerializeField, Tooltip("Scriptable Object contenant les Audio Clip de l'arme (exemple dans le dossier Assets/SoudData)")] 
        public SoundsDataSO _soundData;
        [SerializeField, Tooltip("Effet de tir du bout du canon de l'arme")] public VisualEffect _muzzleFlash; // test

        [SerializeField, Tooltip("Camera shake setting : {x = duration du shake || y = magnitude du shake}")] Vector2 _cameraShakeSettings = new Vector2(0.05f, 0.1f);
        
        private bool ShootingInputPressed;

        [HideInInspector] public bool p_authorizedToShoot = true;

        private EventBus _bus;

        private void Awake()
        {
            _bus = EventBusInitialiser.instance.Bus;
            
            //On récupere tout les types de modules possible et potentiellement sur l'arme
            _shootModule = GetComponents<IShootModule>();
            _reloadModule = GetComponent<IReloadModule>();
            _recoilModule = GetComponent<IRecoilModule>();
            _hitMarkerModule = GetComponent<IHitMarkerModule>();
            _chargedModule = GetComponent<ChargedParentModule>();

            //On initialise tout les modules de l'arme
            foreach (GunModule module in GetComponents<GunModule>())
            {
                module.Initialize(this);
            }
        }

        public void TryFire()
        {
            if (_chargedModule != null)
                if (_chargedModule.IsCharging) return;
            
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
                    _recoilModule?.Recoil(_model.transform, 0.1f, false);
                    _recoilModule?.SetIsRecoil(true);
                    SetAmmo(GetCurrentAmmo() - 1);
                    PlayMuzzleFlash();
                }
            }
            
            _bus.InvokeEvent(new OnCameraShakeEvent
            {
                player = NetworkObject,
                duration = _cameraShakeSettings.x,
                magnitude = _cameraShakeSettings.y
            });

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
                _recoilModule?.Recoil(_model.transform, s.FireRate, true);
                _recoilModule?.SetIsRecoil(true);
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
            
            _recoilModule?.SetIsRecoil(false);
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

            AudioClip clip = SoundManager.GetAudioClip(_soundData,"Reload");
            SoundManager.PlaySound(clip, _source, 0.5f);
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

        public void TryCharging()
        {
            _chargedModule?.TryCharging();
        }

        public void TryShootCharged()
        {
            _chargedModule?.TryShootCharging();
        }

        public void Disable(bool state)
        {
            _model.gameObject.SetActive(state);
        }

        [ObserversRpc]
        private void PlayMuzzleFlash()
        {
            _muzzleFlash.Play();
        }

        public void StopReload() => _reloadModule.StopReload();
    }
}