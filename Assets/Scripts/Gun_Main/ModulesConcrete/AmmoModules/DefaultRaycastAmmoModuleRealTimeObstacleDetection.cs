using System.Collections;
using UnityEngine;

namespace GunDecorator.AmmoModules
{
    public class DefaultRaycastAmmoModuleRealTimeObstacleDetection : GunModule , IAmmoModule
    {
        #region variables
        
        [Header("references")]
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject BulletPrefab;

        [Header("parametres")]
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _damages;
        [SerializeField] private float _BulletSpeed = 50;
        
        [Header("Debug")]
        public GameObject p_markPrefab;
        
        //privates
        private Transform _camTransform;
        
        #endregion

        void Start()
        {
            _camTransform = _camera.transform;
        }


        private Vector3 bulletDirection;
        private float travelTime;
        public void SpawnBullet()
        {
            if (Physics.Raycast(_camTransform.position + transform.forward * .3f, _camTransform.forward, out RaycastHit hit,_maxDistance, ~LayerMask.GetMask("Owner")))
            {
                bulletDirection = (hit.point - (_camTransform.position + transform.forward * .3f)).normalized;
                travelTime = hit.distance / _BulletSpeed;
            }
            else
            {
                bulletDirection = _camera.transform.forward;
                travelTime = _maxDistance /  _BulletSpeed;
            }
            
            GameObject newBullet = Instantiate(BulletPrefab, _camTransform.position + transform.forward * .3f, Quaternion.LookRotation(bulletDirection));
            Destroy(newBullet, travelTime + .5f);
            BulletBehaviour bulletBehaviour = newBullet.GetComponent<BulletBehaviour>();
            bulletBehaviour.p_damage =  _damages;
            bulletBehaviour.p_speed =  _BulletSpeed;
            bulletBehaviour.p_markPrefab =  p_markPrefab;
        }

        public void SetDamage(float multiplierDmg)
        {
            _damages *= multiplierDmg;
        }
    }
}
