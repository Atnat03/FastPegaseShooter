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
    
    public void Recoil(Transform model, float time, float multiplier = 1)
    {
        _targetRotation +=  new Vector3( -_recoilX, Random.Range(-_recoilY, _recoilY) , Random.Range(-_recoilZ, _recoilZ)) * multiplier;
        
        if(model != null)
            StartCoroutine(RecoilingZ(model, time));
    }

    IEnumerator RecoilingZ(Transform model, float time)
    {
        float elapsedTime = 0;
                
        Vector3 startPos = model.transform.localPosition;
        
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / time;
            
            float recoil = Z_RecoilCurve.Evaluate(t) * Z_RecoilDistance;

            model.transform.localPosition = startPos + new Vector3(0, 0, -recoil);

            yield return null;
        }
                
        model.transform.localPosition = startPos;
    }


    public AnimationCurve Z_RecoilCurve => _z_recoilCurve;
    public float Z_RecoilDistance => _z_recoilDistance;
    
    [SerializeField] private AnimationCurve _z_recoilCurve;
    [SerializeField] private float _z_recoilDistance;

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
