using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator.ChargedModules;
using MyPrint;
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
    public void SetFireRate(float multiplier);
    public void SetChargedPlayer(bool b);
}


public interface ISurcharge
{
    public int GetCurrentAmmo();
    public void SetAmmo(int value, bool _infiniteAmmo);
    public void SetSurchargeStat(bool isOverload, float dmgMultiplicator, float cadenceMultiplicator);
    public Renderer ModelGun { get; }
    public void StopReload();
}

namespace GunDecorator
{
    public class GunController : NetworkBusListener, IGun, ISurcharge
    {
        public bool IsOverload => _isOverload.Value;
        public float SurchargeMultiplierDamage { get; set; }
        public float SurchargeMultiplierRate { get; set; }
        public bool IsFullAuto => _isFullAuto;
        public bool IsInfiniteAmmo => _infiniteAmmo;

        public bool IsPositivePlayerCharge => _isPositivePlayerCharge.Value;
        
        public IRecoilModule RecoilModule => _recoilModule;
        
        public Renderer ModelGun => _model;

        private IShootModule _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;
        private IHitMarkerModule _hitMarkerModule;
        private ChargedParentModule _chargedModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);

        [SerializeField, Tooltip("ScriptableObject contenant les settings de l'arme équilibré")]
        private GunModuleSettingsSO _settings;
        
        [SerializeField, Tooltip("Model 3d de l'arme")] 
        private Renderer _model;
        [SerializeField, Tooltip("Audio Source de l'arme")] 
        public AudioSource _source;
        [SerializeField, Tooltip("Scriptable Object contenant les Audio Clip de l'arme (exemple dans le dossier Assets/SoudData)")] 
        public SoundsDataSO _soundData;
        
        [SerializeField, Tooltip("Animation du modele de l'arme")] 
        public Animator _animator;
        
        [SerializeField, Tooltip("Effet de tir du bout du canon de l'arme")] public VisualEffect _muzzleFlash; // test
        [SerializeField] [Tooltip("est ce que le maintient du clic provoque un tir automatique")]private bool _isFullAuto;
        
        private bool ShootingInputPressed = true;
        private float _fireRateMultiplier = 1;
        private bool _infiniteAmmo = false;
        private readonly SyncVar<bool> _isPositivePlayerCharge = new SyncVar<bool>(false);

        [HideInInspector] public bool p_authorizedToShoot = true;

        private void OnEnable()
        {
            _animator.ResetTrigger("Reload");
        }

        private void Awake()
        {
            //On récupere tout les types de modules possible et potentiellement sur l'arme
            _shootModule = GetComponent<IShootModule>();
            _reloadModule = GetComponent<IReloadModule>();
            _recoilModule = GetComponent<IRecoilModule>();
            _hitMarkerModule = GetComponent<IHitMarkerModule>();
            _chargedModule = GetComponent<ChargedParentModule>();

            List<GunModule> modules = GetComponents<GunModule>().ToList();

            //On initialise tout les modules de l'arme
            foreach (GunModule module in modules)
            {
                module.Initialize(this);
            }

            if(_settings != null)
            {
                foreach (GunSetting s in _settings.modulesList)
                {
                    GunModule found = modules.Find(x => x.GetType().Name == s.displayName);

                    if (found != null)
                    {
                        found.SetVariable(s);
                    }
                }
            }
        }

        public void TryFire()
        {
            if (_chargedModule != null)
                if (_chargedModule.IsCharging) return;
    
            ShootingInputPressed = true;
            ApplyShoot();
        }

        public void ApplyShoot()
        {
            if (!ShootingInputPressed) return;
            
            if (GetCurrentAmmo() > 0 && !_reloadModule.IsReloading && p_authorizedToShoot)
            {
                if (!_shootModule.CanShoot) return;
                
                _shootModule.SetFireRate(_fireRateMultiplier);
                    
                if (IsFullAuto)
                {
                    StartCoroutine(ShootingCoroutine(_shootModule));
                    return;
                }
                _shootModule?.TryShoot();
                    
                _recoilModule?.Recoil(_model.transform, 0.1f, false);
                _recoilModule?.SetIsRecoil(true);
                    
                SetAmmo(GetCurrentAmmo() - 1, _infiniteAmmo);
                PlayMuzzleFlash();
                    
                _animator?.SetTrigger("Shoot");
            }
        }


        IEnumerator ShootingCoroutine(IShootModule s)
        {
            while (ShootingInputPressed && GetCurrentAmmo() > 0 && !_reloadModule.IsReloading)
            {
                _shootModule.SetFireRate(_fireRateMultiplier);
                
                p_authorizedToShoot = false;
                s.TryShoot();
                PlayMuzzleFlash();
                
                _recoilModule?.Recoil(_model.transform, s.FireRate, true);
                _recoilModule?.SetIsRecoil(true);
                
                SetAmmo(GetCurrentAmmo() - 1, _infiniteAmmo);
                
                _animator?.SetTrigger("Shoot");
                
                yield return new WaitForSeconds(s.FireRate);
                
                p_authorizedToShoot =  true;
            }
        }
        

        public void TryCancelShooting()
        {
            ShootingInputPressed = false;
            _shootModule?.CancelShooting();
            
            _recoilModule?.SetIsRecoil(false);
            
            p_authorizedToShoot = true;
        }

        public int GetCurrentAmmo() => _reloadModule.CurrentAmmo;

        public void SetAmmo(int value, bool infiniteAmmo) => _reloadModule.SetAmmo(value, _infiniteAmmo);

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
            _shootModule?.AmmoModule.SetDamage(dmgMultiplicator);
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

        public void SetFireRate(float multiplier)
        {
            _fireRateMultiplier = multiplier;
            
            _infiniteAmmo = multiplier == -1 ? false : true;
        }

        public void PlaySound(string sound)
        {
            AudioClip clip = SoundManager.GetAudioClip(_soundData,sound);

            if (clip == null) return;
            
            SoundManager.PlaySound(clip, _source, 0.5f);
            
            PlaySoundServerRpc(sound);
        }

        [ServerRpc]
        void PlaySoundServerRpc(string sound)
        {
            PlaySoundObserverRpc(sound);
        }

        [ObserversRpc(ExcludeOwner = true)]
        void PlaySoundObserverRpc(string sound)
        {
            AudioClip clip = SoundManager.GetAudioClip(_soundData,sound);
            SoundManager.PlaySound(clip, _source, 0.5f);
        }

        [ObserversRpc]
        private void PlayMuzzleFlash()
        {
            _muzzleFlash.Play();
        }

        public void StopReload() => _reloadModule.StopReload();

        public void SetChargedPlayer(bool b) => _isPositivePlayerCharge.Value = b;
    }
}