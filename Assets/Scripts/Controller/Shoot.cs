using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : NetworkBehaviour
{
    #region  Properties

    public GameObject GunVisual => p_visualWeapon;
    
    #endregion
    
    #region Variables

    [Header("references")]
    [SerializeField] private PlayerInput _playerInputAction;
    [SerializeField] private Transform _playerHandPosWhenStandUp;
    [SerializeField] private Transform _playerHandPosWhenCrouched;
    [SerializeField] private Transform _targetPos;
    
    public MainWeaponsSO p_weaponData; //le serialiser pour l'instant
    public Action p_shootingAction;

    public GameObject p_visualWeapon; // utile uniquement pour le feedBack temporaire

    private Transform _camTransform;
    
    #endregion

    public override void OnStartClient() // fonction de Debug qui part du principe qu'on renseigne la premiere arme
    {
        base.OnStartClient();
        _camTransform =  Camera.main.transform;
        InitNewWeapon(p_weaponData);
    }

    void OnEnable()
    {
        _playerInputAction.actions["Shoot"].performed += Shooting;
    }

    void OnDisable()
    {
        _playerInputAction.actions["Shoot"].performed -= Shooting;
    }

    void InitNewWeapon(MainWeaponsSO newWeaponSO)
    {
        p_weaponData = newWeaponSO;
        ClearChildren(_targetPos);
        p_visualWeapon = Instantiate(newWeaponSO.p_weaponVisual, _targetPos.position, Quaternion.identity, _targetPos);
    }
    
    private void Shooting(InputAction.CallbackContext obj)
    {
        p_shootingAction?.Invoke();
        FeedBackShooting(); // utile uniquement pour le feedBack temporaire
    }

    void Update() // utile uniquement pour le feedBack temporaire
    {
        _targetPos.rotation = _camTransform.rotation;
        BringBackWeapon();
    }

    void BringBackWeapon()// utile uniquement pour le feedBack temporaire
    {
        p_visualWeapon.transform.position = Vector3.Lerp(p_visualWeapon.transform.position, _targetPos.position, p_weaponData.p_recoilOffsetCompensation *  Time.deltaTime);
        p_visualWeapon.transform.rotation = Quaternion.Slerp(p_visualWeapon.transform.rotation, _targetPos.rotation, p_weaponData.p_recoilTorkCompensation * Time.deltaTime);
    }

    private void FeedBackShooting() // FONCTION TEMPORAIRE DE TESTS DE FEEDBACKS
    {
        Recoil();// utile uniquement pour le feedBack temporaire
    }

    private void Recoil()// utile uniquement pour le feedBack temporaire
    {
        p_visualWeapon.transform.Translate(new Vector3( 0f, 0f, -p_weaponData.p_recoilOffsetIntensity), Space.Self);
        p_visualWeapon.transform.localRotation *= Quaternion.Euler( -p_weaponData.p_recoilTorkIntensity, 0f,0f);
    }
    
    void ClearChildren(Transform t)
    {
        foreach (Transform child in t)
        {
            Destroy(child.gameObject);
        }
    }
}