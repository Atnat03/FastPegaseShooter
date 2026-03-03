using System;
using GunDecorator;
using UnityEngine;
using Random = UnityEngine.Random;

public class RecoilModule : GunModule, IRecoilModule
{
    #region variables

    // serializable variables
    [Header("References")] [SerializeField]
    private Transform _recoilTransform;

    [Header("Settings")] 
    [SerializeField]private float _recoilX;
    [SerializeField]private float _recoilY;
    [SerializeField]private float _recoilZ;
    
    [SerializeField] private float _returnSpeed = 10f;
    [SerializeField] private float _snapiness = 10f;
    
    
    //privates variables
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    #endregion
    
    public void Recoil()
    {
        _targetRotation +=  new Vector3( -_recoilX, Random.Range(-_recoilY, _recoilY) , Random.Range(-_recoilZ, _recoilZ));
    }

    void Update()
    {
        BringBackWeapon();
    }
    
    void BringBackWeapon()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snapiness * Time.deltaTime);
        _recoilTransform.localRotation = Quaternion.Euler(_currentRotation);
    }
}
