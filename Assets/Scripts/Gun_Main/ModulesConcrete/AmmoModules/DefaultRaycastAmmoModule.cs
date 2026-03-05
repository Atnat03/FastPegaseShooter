using System.Collections;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class DefaultRaycastAmmoModule : GunModule , IAmmoModule
    {
        #region variables
        
        [Header("references")]
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _visualBulletPrefab;

        [Header("parametres")]
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _damages;
        [SerializeField] private float _BulletSpeed = 50;
        
        [Header("Debug")]
        public GameObject p_markPrefab;
        private GameObject _currentMark;
        
        //privates
        private Transform _camTransform;
        
        #endregion

        void Start()
        {
            _camTransform = _camera.transform;
        }


        private Vector3 _spawnPos;
        public void SpawnBullet()
        {
            _spawnPos = _camTransform.position + transform.forward * .3f;
            if (Physics.Raycast(_spawnPos, _camTransform.forward, out RaycastHit hit,_maxDistance, ~LayerMask.GetMask("Owner"), QueryTriggerInteraction.Ignore))
            {
                if (_currentMark != null)
                {
                    Destroy(_currentMark);
                }
                
                float travelTime = hit.distance / _BulletSpeed;
                
                StartCoroutine(TravelTimeCoroutine(hit, travelTime, Instantiate(_visualBulletPrefab,transform.position + transform.forward * .3f, Quaternion.identity), _spawnPos));
            }
        }

        IEnumerator TravelTimeCoroutine(RaycastHit hit, float travelTime, GameObject bullet, Vector3 spawnPos)
        {
            float elapsedTime = 0;
            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                bullet.transform.position += (hit.point - spawnPos).normalized * (_BulletSpeed * Time.deltaTime);
                yield  return null;
            }
            Destroy(bullet);
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
