using System;
using System.Collections;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField]private GameObject _canvas;
    
    [HideInInspector]public Transform p_playerTransform;
    [HideInInspector]public bool p_mustShowCanvas = false;
     public Transform p_targetTransform;

    void Start()
    {
        p_playerTransform = Camera.main?.transform;
        _canvas.SetActive(false);
    }
    
    void Update() 
    {
        if(p_playerTransform!=null) _canvas.transform.LookAt(p_playerTransform);
        _canvas.SetActive(p_mustShowCanvas); 
    }
    
}
