using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : NetworkBehaviour
{
    EventBus _bus;
    private Coroutine _currentShake;
    private Vector3 _initialLocalPos;
    
    public void Awake()
    {
        _bus = EventBusInitialiser.instance.Bus;
        
        _initialLocalPos = transform.localPosition;

        _bus.Subscribe((OnCameraShakeEvent data) => Shake(data));
    }

    public void Shake(OnCameraShakeEvent data)
    {
        if (_currentShake != null)
            StopCoroutine(_currentShake);

        _currentShake = StartCoroutine(Shaking(data.duration, data.magnitude));
    }

    IEnumerator Shaking(float duration, float magnitude)
    {
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = _initialLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _initialLocalPos;
        _currentShake = null;
    }
}

public struct OnCameraShakeEvent
{
    public NetworkObject player;
    public float duration;
    public float magnitude;
}
