using System;
using System.Collections;
using UnityEngine;

public class Ascenseur : MonoBusListener
{
    private Coroutine _currentCoroutine;
    private Vector3 _offset;
    private bool elevatorGoing = false;

    void Start()
    {
        ListenToEvent<OnDapEvent>(StopElevator);
    }
    
    public void StartDescente(Vector3 startPosition, Vector3 endPosition, float duration, float timeOffset = 0f)
    {
        gameObject.SetActive(true);

        Vector3 rail = endPosition - startPosition;
        Vector3 railDir = rail.normalized;

        Vector3 lateralOffset = transform.position - startPosition;
        lateralOffset -= Vector3.Dot(lateralOffset, railDir) * railDir;

        Vector3 localStart = startPosition + lateralOffset;
        Vector3 localEnd   = endPosition   + lateralOffset;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(DescenteAscenseur(localStart, localEnd, duration, timeOffset));
    }

    private IEnumerator DescenteAscenseur(Vector3 localStart, Vector3 localEnd, float duration, float startElapsed)
    {
        float elapsed = startElapsed % duration; // sécurité si timeOffset > duration

        while (true)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(localStart, localEnd, elapsed / duration);
                yield return null;
            }

            elapsed = 0f;
            transform.position = localStart;
        }
    }
    
    void StopElevator(OnDapEvent e)
    {
        elevatorGoing = false;
    }
}