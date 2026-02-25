using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private GameObject _visualWeapon;

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
        Instantiate(newWeaponSO.p_weaponVisual, _targetPos.position, _targetPos.rotation, _targetPos);
    }
    
    private void Shooting(InputAction.CallbackContext obj)
    {
        p_shootingAction?.Invoke();
        Debug.Log("shooting");
    }

    void ClearChildren(Transform t)
    {
        foreach (Transform child in t)
        {
            Destroy(child.gameObject);
        }
    }
}
