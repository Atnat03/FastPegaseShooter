using System;
using System.Collections;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField]private GameObject _canvas;
    private Transform _camTransform;

    [HideInInspector]public bool p_mustShowCanvas = false;

    void Start()
    {
        _camTransform = Camera.main?.transform;
        _canvas.SetActive(false);
    }
    
    void Update() 
    {
        if(_camTransform!=null) transform.LookAt(_camTransform);
        _canvas.SetActive(p_mustShowCanvas); // ne se desactive jamais
    }
    
}
