using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Shoot : MonoBehaviour
{
    #region Variables

    [Header("references")]
    [SerializeField] private PlayerInput _playerInputAction;
    [SerializeField] private Transform _playerHandPosWhenStandUp;
    [SerializeField] private Transform _playerHandPosWhenCrouched;
    [SerializeField] private Transform _targetPos;
    
    public MainWeaponsSO p_weaponData; //le serialiser pour l'instant
    public Action p_shootingAction;

    private GameObject _visualWeapon; // utile uniquement pour les feedbacks pour l'instant

    #endregion

    private void Start() // fonction de Debug qui part du principe qu'on renseigne la premiere arme
    {
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
        _visualWeapon = Instantiate(newWeaponSO.p_weaponVisual, _targetPos.position, _targetPos.rotation, _targetPos);
    }
    
    private void Shooting(InputAction.CallbackContext obj)
    {
        p_shootingAction?.Invoke();
        FeedBackShooting();
    }

    void Update() // utile uniquement pour le feedBack temporaire
    {
        BringBackWeapon();
    }

    void BringBackWeapon()
    {
        _visualWeapon.transform.position = Vector3.Lerp(_visualWeapon.transform.position, _targetPos.position, p_weaponData.p_recoilOffsetCompensation *  Time.deltaTime);
        _visualWeapon.transform.rotation = Quaternion.Slerp(_visualWeapon.transform.rotation, _targetPos.rotation, p_weaponData.p_recoilTorkCompensation * Time.deltaTime);
    }

    private void FeedBackShooting() // FONCTION TEMPORAIRE DE TESTS DE FEEDBACKS
    {
        Recoil();
    }

    private void Recoil()
    {
        _visualWeapon.transform.position += new Vector3(-p_weaponData.p_recoilOffsetIntensity,0,0);
        _visualWeapon.transform.localRotation *= Quaternion.Euler( 0f,0f, p_weaponData.p_recoilTorkIntensity);
    }
    
    void ClearChildren(Transform t)
    {
        foreach (Transform child in t)
        {
            Destroy(child.gameObject);
        }
    }
}
