using System;
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
    private float _recoilX;

    [SerializeField, Tooltip("Amplitude aléatoire du recul horizontal gauche/droite (axe Y).")]
    private float _recoilY;

    [SerializeField, Tooltip("Amplitude aléatoire de rotation latérale / tilt de l'arme (axe Z).")]
    private float _recoilZ;
    
    [SerializeField, Tooltip("Vitesse à laquelle le recul revient progressivement à la position initiale.")]
    private float _returnSpeed = 10f;

    [SerializeField, Tooltip("Vitesse à laquelle l'arme atteint la rotation de recul cible (plus la valeur est grande, plus le recul est sec).")]
    private float _snapiness = 10f;
    
    
    //privates variables
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    #endregion
    
    public void Recoil(float multiplier = 1)
    {
        _targetRotation +=  new Vector3( -_recoilX, Random.Range(-_recoilY, _recoilY) , Random.Range(-_recoilZ, _recoilZ)) * multiplier;
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
