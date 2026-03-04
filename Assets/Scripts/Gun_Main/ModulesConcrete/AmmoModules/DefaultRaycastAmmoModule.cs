using System;
using System.Collections;
using Controller;
using CustomConsole.Runtime.Logger;
using FishNet;
using Unity.VisualScripting;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class DefaultRaycastAmmoModule : GunModule , IAmmoModule
    {
        #region variables

        [Header("parametres")]
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _damages;
        [SerializeField] private float _BulletSpeed = 50;
        [SerializeField] private Camera _camera;
        
        [Header("Debug")]
        public GameObject p_markPrefab;
        private GameObject _currentMark;
        private Transform _camTransform;
        
        #endregion

        private PlayerShooting _ps;

        void Start()
        {
            _camTransform = _camera.transform;
            _ps = GetComponentInParent<PlayerShooting>();
        }
        
        public void SpawnBullet()
        {
            if (Physics.Raycast(_camTransform.position + transform.forward * .3f, transform.forward, out RaycastHit hit,_maxDistance, ~LayerMask.GetMask("Owner")))
            {
                if (_currentMark != null)
                {
                    Destroy(_currentMark);
                }
                
                float travelTime = hit.distance / _BulletSpeed;
                
                StartCoroutine(TravelTimeCoroutine(hit, travelTime));
            }
        }

        IEnumerator TravelTimeCoroutine(RaycastHit hit, float travelTime)
        {
            yield return new WaitForSeconds(travelTime);
            HitTarget(hit);
        }

        private void HitTarget(RaycastHit hit)
        {
            _currentMark = Instantiate(p_markPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
            if (hit.collider.TryGetComponent<IDamagable>(out IDamagable iDamagable))
            {
                iDamagable.TakeDamage((int)_damages);
            }
        }

        public void SetDamage(float multiplierDmg)
        {
            _damages *= multiplierDmg;
        }
    }
}
