using System;
using System.Threading.Tasks;
using UnityEngine;

public class ArenaDoorViewer : MonoBehaviour
{
    [SerializeField] private ArenaDoor _arenaDoor;
    
    [SerializeField] private Transform _doorPivotPoint;
    [SerializeField] private float _openingSpeed = 2;
    [SerializeField] private AnimationCurve _openingCurve;
    [SerializeField] private float _openingAngle = 90;
    private float _t;
    private bool _isOpening = false;

    private void Awake()
    {
        _arenaDoor.p_onDoorOpening += OnDoorOpening;
    }

    private void OnDoorOpening()
    {
        if(!_isOpening)OpenDoor();
    }

    async void OpenDoor()
    {
        _isOpening = true;
        Quaternion initialRotation = _doorPivotPoint.rotation;
        _t = 0;
        while (_t < 1)
        {
            _doorPivotPoint.rotation = Quaternion.Slerp(
                initialRotation,
                Quaternion.Euler(0, _openingAngle, 0),
                _openingCurve.Evaluate(_t));
            _t += Time.deltaTime * _openingSpeed;
            await Task.Yield();
        }
        _isOpening=false;
    }
}
