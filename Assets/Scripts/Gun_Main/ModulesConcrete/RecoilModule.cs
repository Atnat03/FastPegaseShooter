using System;
using GunDecorator;
using UnityEngine;

public class RecoilModule : GunModule, IRecoilModule
{
    #region variables

    [Header("References")]
    [SerializeField] Transform targetPosition;

    #endregion
    
    public void Recoil()
    {
        Debug.Log("Recoil");
    }

    public void OnEnable()
    {
        transform.position = targetPosition.position;
    }

    void Update()
    {
        
    }
}
