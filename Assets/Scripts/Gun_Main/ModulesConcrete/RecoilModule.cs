using System;
using System.Collections;
using GunDecorator;
using UnityEngine;
using Random = UnityEngine.Random;

public class RecoilModule : GunModule, IRecoilModule
{
    #region variables

    // serializable variables
    [Header("References")] [SerializeField, Tooltip("Objet CameraRecoil dans la hiérarchie du joueur")]
    private Transform _recoilTransform;

    [Header("Settings")]
    
    [SerializeField, Tooltip("Force du recul vertical (axe X). Valeur négative appliquée pour faire monter l'arme.")]
    private AnimationCurve _recoilCurve;
    
    [SerializeField, Tooltip("Amplitude aléatoire du recul horizontal gauche/droite (axe Y).")]
    private float _recoilY;

    [SerializeField, Tooltip("Amplitude aléatoire de rotation latérale / tilt de l'arme (axe Z).")]
    private float _recoilZ;
    
    [SerializeField, Tooltip("Vitesse à laquelle le recul revient progressivement à la position initiale.")]
    private float _returnSpeed = 10f;

    [SerializeField, Tooltip("Vitesse à laquelle l'arme atteint la rotation de recul cible (plus la valeur est grande, plus le recul est sec).")]
    private float _snapiness = 10f;
    
    [Header("Z Kickback")]
    [SerializeField]
    private float _z_recoilDistance = 0.05f;

    [SerializeField]
    private float _z_returnSpeed = 8f;

    [SerializeField]
    private float _maxZKickback = 0.15f;

    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    private float _currentZ;
    private float _targetZ;
    private float _timeRecoil = 0;
    private bool _isRecoil = false;
    private float _maxRecoilTime = 10;

    private Vector3 _initialLocalPos;
    
    #endregion

    void Start()
    {
        _initialLocalPos = _recoilTransform.localPosition;
    }

    public void Recoil(Transform model, float time, float multiplier = 1)
    {
        _targetRotation += new Vector3(
            -_recoilCurve.Evaluate(_timeRecoil),
            Random.Range(-_recoilY, _recoilY),
            Random.Range(-_recoilZ, _recoilZ)
        ) * multiplier;

        _targetZ += _z_recoilDistance * multiplier;

        _targetZ = Mathf.Clamp(_targetZ, 0, _maxZKickback);

    }

    void Update()
    {
        if(_recoilCurve.length > 0)
            _maxRecoilTime = _recoilCurve[_recoilCurve.length-1].time;
        
        if (_isRecoil && _timeRecoil < _maxRecoilTime)
        {
            _timeRecoil += Time.deltaTime * _snapiness;
        }
        else if(_timeRecoil > 0)
        {
            _timeRecoil -= Time.deltaTime * _returnSpeed * 2;
        }
        
        BringBackWeapon();
    }

    void BringBackWeapon()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snapiness * Time.deltaTime);
        _recoilTransform.localRotation = Quaternion.Euler(_currentRotation);

        _targetZ = Mathf.Lerp(_targetZ, 0, _z_returnSpeed * Time.deltaTime);
        _currentZ = Mathf.Lerp(_currentZ, _targetZ, _snapiness * Time.deltaTime);

        _gunController.ModelGun.transform.localPosition = _initialLocalPos + new Vector3(0, 0, -_currentZ);
    }

    public void SetIsRecoil(bool state)
    {
        _isRecoil = state;
    }
}
