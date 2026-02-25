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
    [SerializeField] private Transform _WeaponVisualParent;
    
    public MainWeaponsSO p_weaponData; //pour le serialiser pour l'instant

    public Action p_shootingAction;

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
        ClearChildren(_WeaponVisualParent);
        Instantiate(newWeaponSO.p_weaponVisual, _WeaponVisualParent.position, Quaternion.identity, _WeaponVisualParent);
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
