using System;
using System.Collections;
using UnityEngine;

public class Ascenseur : MonoBusListener
{
    private Coroutine _currentCoroutine;
    private bool _elevatorGoing = false;
    private float _speed;
    private Vector3 _localStart;
    private Vector3 _railDir;
    private float _doorInterval;
    private Action<Ascenseur> _onAtDoor;

    void Start()
    {
        ListenToEvent<OnDapEvent>(e => StopElevator());
    }
    

    public void StartDescente(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        gameObject.SetActive(true);
        _elevatorGoing = true;

        float distance = Vector3.Distance(startPosition, endPosition);
        _railDir = (endPosition - startPosition).normalized;
        _speed = distance / duration;

        Vector3 lateralOffset = transform.position - startPosition;
        lateralOffset -= Vector3.Dot(lateralOffset, _railDir) * _railDir;

        _localStart = startPosition + lateralOffset;
        Vector3 localEnd = endPosition + lateralOffset;

        float progress = Vector3.Dot(transform.position - startPosition, _railDir) / distance;
        float startElapsed = Mathf.Clamp01(progress) * duration;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(DescenteAscenseur(_localStart, localEnd, duration, startElapsed));
    }

    private bool IsAtDoorHeight()
    {
        float distFromTop = Vector3.Dot(transform.position - _localStart, _railDir);
        float remainder = distFromTop % 63.639f;
        float frameTolerance = _speed * Time.deltaTime + 0.05f;
        return remainder < frameTolerance;
    }

    private IEnumerator DescenteAscenseur(Vector3 localStart, Vector3 localEnd, float duration, float startElapsed)
    {
        float elapsed = startElapsed;

        while (true)
        {
            // Move until end of rail, but stop mid-loop if elevator should stop AND we're at a floor
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(localStart, localEnd, elapsed / duration);

                if (!_elevatorGoing && IsAtDoorHeight())
                {
                    // Snap cleanly to the nearest floor
                    float distFromTop = Vector3.Dot(transform.position - localStart, _railDir);
                    float snapped = Mathf.Round(distFromTop / 63.63f) * 63.63f;
                    transform.position = localStart + _railDir * snapped;
                    yield break; // stop the coroutine entirely
                }

                yield return null;
            }

            // Looped back to top
            elapsed = 0f;
            transform.position = localStart;
            OnLoop();
        }
    }
    
    public void StopElevator()
    {
        _elevatorGoing = false;
    }

    protected virtual void OnLoop() { }
}