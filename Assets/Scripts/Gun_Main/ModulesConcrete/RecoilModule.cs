using System;
using GunDecorator;
using UnityEngine;

public class RecoilModule : GunModule, IRecoilModule
{
    #region variables

    [Header("References")]
    [SerializeField] Transform _targetPosition;
    private Transform _camTransform;

    [Header("Settings")] 
    [SerializeField] float _recoilOffsetIntensity = 0.6f;
    [SerializeField] float _recoilTorkIntensity = 45f;
    
    [SerializeField] private float _recoilOffsetCompensationSpeed = 10f;
    [SerializeField] private float _recoilTorkCompensationSpeed = 10f;

    #endregion
    
    public void Recoil()
    {
        transform.Translate(new Vector3( 0f, 0f, -_recoilOffsetIntensity), Space.Self);
        transform.localRotation *= Quaternion.Euler( -_recoilTorkIntensity, 0f,0f);
        _camTransform.localRotation *= Quaternion.Euler( -_recoilTorkIntensity / 5, 0f,0f); // ignoble
    }

    public void OnEnable()
    {
        transform.position = _targetPosition.position;
    }

    public void Start()
    {
        _camTransform =  Camera.main.transform;
    }

    void Update() // utile uniquement pour le feedBack temporaire
    {
        _targetPosition.rotation = _camTransform.rotation;
        BringBackWeapon();
    }
    
    void BringBackWeapon()
    {
        transform.position = Vector3.Lerp(transform.transform.position, _targetPosition.position, _recoilOffsetCompensationSpeed *  Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetPosition.rotation, _recoilTorkCompensationSpeed * Time.deltaTime);
    }
}
