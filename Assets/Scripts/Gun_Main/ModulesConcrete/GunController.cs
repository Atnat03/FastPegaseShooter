using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomConsole.Runtime.Logger;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator.ChargedModules;
using Managers;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.VFX;

public interface IGun
{
    public void TryFire();
    public void TryCancelShooting();
    public void TryReload();
    public void TryShootCharged();
    public void Disable(bool state);
    public void SetFireRate(float multiplier);
    public void SetInfiniteAmmo(bool infiniteAmmo);
    public void SetChargedPlayer(bool b);
    public void SetReticule(ReticulesManager manager);
}


public interface ISurcharge
{
    public int GetCurrentAmmo();
    public void SetAmmo(int value, bool _infiniteAmmo);
    public Transform CurrentModelGun { get; }
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
        public Transform CurrentModelGun => currentModel;

        private IShootModule _shootModule;
        private IReloadModule _reloadModule;
        private IRecoilModule _recoilModule;
        private IHitMarkerModule _hitMarkerModule;
        private ChargedParentModule _chargedModule;

        private readonly SyncVar<bool> _isOverload = new SyncVar<bool>(false);

        [SerializeField, Tooltip("ScriptableObject contenant les settings de l'arme équilibré")]
        private GunModuleSettingsSO _settings;

        [SerializeField, Tooltip("Model 3d de l'arme")]
        private Transform currentModel;

        [SerializeField, Tooltip("Model 3d de l'arme suivant la charge")]
        private Transform[] _modelsList;

        [SerializeField, Tooltip("Audio Source de l'arme")]
        public AudioSource _source;

        [SerializeField, Tooltip("Scriptable Object contenant les Audio Clip de l'arme (exemple dans le dossier Assets/SoudData)")]
        public SoundsDataSO _soundData;

        [HideInInspector, Tooltip("Animation du modele de l'arme")]
        public Animator _animator;
        
        [SerializeField, Tooltip("Animation du bras")]
        public Animator _animatorArm;

        [SerializeField, Tooltip("Effet de tir du bout du canon de l'arme")]
        public ParticleSystem[] p_particlesMuzzleFlash;

        [SerializeField] [Tooltip("est ce que le maintient du clic provoque un tir automatique")]
        private bool _isFullAuto;

        [SerializeField] private int _reticuleID = 0;

        [SerializeField] private PlayerZoneManager playerZoneManager;

        private bool ShootingInputPressed = true;
        private float _fireRateMultiplier = 1;
        private bool _infiniteAmmo = false;
        private readonly SyncVar<bool> _isPositivePlayerCharge = new SyncVar<bool>(false);

        [HideInInspector] public bool p_authorizedToShoot = true;

        //Action

        //Shoot
        public Action<int, int> OnShootAmmo;
        public Action<float> OnShootNoise;

        //Reloading
        public Action<float> OnStartReload;
        public Action OnEndReload;

        //Charging
        public Action<float> OnCharging;
        public Action OnStopCharging;

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

            if (_settings != null)
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

        public override void OnStartClient()
        {
            if(_modelsList.Length <2)
                return;
            
            foreach (Transform t in _modelsList)
            {
                t.gameObject.SetActive(false);
            }
            
            _modelsList[LocalConnection.ClientId].gameObject.SetActive(true);

            if (_modelsList[LocalConnection.ClientId].TryGetComponent(out Animator animator))
                _animator = animator;
        }

        private void Start() // pour du debug, a tej en build finale
        {
            playerZoneManager = FindAnyObjectByType<PlayerZoneManager>();
        }

        public void TryFire()
        {
            ShootingInputPressed = true;
            ApplyShoot();
        }

        public void ApplyShoot()
        {
            if (!ShootingInputPressed) return;

            if (!currentModel.gameObject.activeInHierarchy)
                return;

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

                _recoilModule?.Recoil(currentModel.transform, 0.1f, false);
                _recoilModule?.SetIsRecoil(true);

                SetAmmo(GetCurrentAmmo() - 1, _infiniteAmmo);
                PlayMuzzleFlash();

                _animator?.SetTrigger("Shoot");
                
                if(_animatorArm)
                    _animatorArm?.SetTrigger("Shoot");
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

                _recoilModule?.Recoil(currentModel.transform, s.FireRate, true);
                _recoilModule?.SetIsRecoil(true);

                SetAmmo(GetCurrentAmmo() - 1, _infiniteAmmo);

                _animator?.SetTrigger("Shoot");
                
                if(_animatorArm)
                    _animatorArm?.SetTrigger("Shoot");

                yield return new WaitForSeconds(s.FireRate);

                p_authorizedToShoot = true;
            }
        }

        public void TryCancelShooting()
        {
            ShootingInputPressed = false;
            _shootModule?.CancelShooting();

            _recoilModule?.SetIsRecoil(false);
        }

        public int GetCurrentAmmo() => _reloadModule.CurrentAmmo;

        public void SetAmmo(int value, bool infiniteAmmo) => _reloadModule.SetAmmo(value, _infiniteAmmo);

        public void TryReload()
        {
            if (_reloadModule.IsReloading) return;

            _reloadModule?.Reload();
        }

        [ServerRpc(RequireOwnership = true)]
        public void RequestApplyDamage(GameObject target, int damage, bool isCritical, bool hadCharged)
        {
            ApplyDamage(target, damage, isCritical, hadCharged);
        }

        public void ApplyDamage(GameObject target, int damage, bool isCritical, bool hadCharged)
        {
            if (target == null) return;
            if (!target.TryGetComponent<IDamagable>(out var d)) return;

            bool crit = d.TakeDamage(OwnerId, damage, IsPositivePlayerCharge.ToChargeType(), isCritical);
            Cons.Print("Damage : " + crit);

            ApplyDamageObservers(damage, isCritical, hadCharged);
            

            // debug clement
            InvokeEvent(new OnDataLog
            {
                entityName = transform.GetRootTransform().gameObject.name,
                EntityID = ObjectId,
                weapon = gameObject.name,
                targetName = target.transform.root.name,
                targetID = target.transform.root.GetComponent<NetworkObject>().ObjectId,
                damages = damage,
                ArenaID = (playerZoneManager != null && playerZoneManager.p_playerZones.ContainsKey(OwnerId)) ? playerZoneManager.p_playerZones[OwnerId] : -1
            });

            // fin du debug 
        }

        [ObserversRpc]
        private void ApplyDamageObservers(int damage, bool isCritical, bool hadCharged)
        {
            InvokeEvent(new OnPlayerDoDamage
            {
                p_ownerId = OwnerId,
                p_value = damage,
                p_critical = isCritical
            });
        }

        public void TryShootCharged()
        {
            _chargedModule?.StartChargedShoot();
        }

        public void Disable(bool state)
        {
            currentModel.gameObject.SetActive(state);
        }

        public void SetFireRate(float multiplier)
        {
            _fireRateMultiplier = multiplier;
        }

        public void SetInfiniteAmmo(bool infiniteAmmo) => _infiniteAmmo = infiniteAmmo;

        public void PlaySound(string sound)
        {
            AudioClip clip = SoundManager.GetAudioClip(_soundData, sound);

            if (clip == null) return;

            SoundManager.PlaySound(_soundData, sound, _source);

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
            SoundManager.PlaySound(_soundData, sound, _source);
        }

        [ObserversRpc]
        private void PlayMuzzleFlash()
        {
            if (p_particlesMuzzleFlash.Length < 2) 
                return;

            if (IsPositivePlayerCharge)
            {
                p_particlesMuzzleFlash[0].Play();
            }
            else
            {
                p_particlesMuzzleFlash[1].Play();
            }
        }

        public void StopReload() => _reloadModule.StopReload();

        public void SetChargedPlayer(bool b) => _isPositivePlayerCharge.Value = b;

        public void ResetNoise() => _shootModule?.CancelShooting();

        public void SetReticule(ReticulesManager manager)
        {
            manager.ActivateReticules(_reticuleID);
        }
        
        public void SetDamage(float ratio) => _shootModule.AmmoModule.SetDamage(ratio);
    }
}