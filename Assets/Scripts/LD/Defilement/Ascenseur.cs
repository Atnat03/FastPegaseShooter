using System;
using System.Collections;
using UnityEngine;

public class Ascenseur : MonoBehaviour
{
    public event Action OnThresholdReached;

    [Range(0f, 1f)]
    [SerializeField] private float _spawnThreshold = 0.5f;

    private Coroutine _currentCoroutine;
    private bool _thresholdTriggered;

    public void StartDescente(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        gameObject.SetActive(true);
        
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _thresholdTriggered = false;
        _currentCoroutine = StartCoroutine(DescenteAscenseur(startPosition, endPosition, duration));
    }

    private IEnumerator DescenteAscenseur(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsed = 0f;
        transform.position = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            if (!_thresholdTriggered && t >= _spawnThreshold)
            {
                _thresholdTriggered = true;
                OnThresholdReached?.Invoke();
            }

            yield return null;
        }

        transform.position = endPosition;
        
        gameObject.SetActive(false);
    }
}