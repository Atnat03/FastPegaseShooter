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
    
    public void StartDescente(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        gameObject.SetActive(true);

        elevatorGoing = true;
        Vector3 rail = endPosition - startPosition;
        Vector3 railDir = rail.normalized;

        // Projection latérale : on garde la composante perpendiculaire au rail
        Vector3 lateralOffset = transform.position - startPosition;
        lateralOffset -= Vector3.Dot(lateralOffset, railDir) * railDir; // on retire la composante sur le rail

        // localStart = en haut du rail + décalage latéral seulement
        Vector3 localStart = startPosition + lateralOffset;
        Vector3 localEnd   = endPosition   + lateralOffset;

        // Avancement initial sur le rail
        float progress = Vector3.Dot(transform.position - startPosition, railDir) / rail.magnitude;
        float startElapsed = Mathf.Clamp01(progress) * duration;

        if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(DescenteAscenseur(localStart, localEnd, duration, startElapsed));
    }

    private IEnumerator DescenteAscenseur(Vector3 localStart, Vector3 localEnd, float duration, float startElapsed)
    {
        float elapsed = startElapsed;

        while (elevatorGoing)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(localStart, localEnd, elapsed / duration);
                yield return null;
            }

            elapsed = 0f;
            transform.position = localStart; // reset en haut du rail propre à cet objet
            OnLoop();
        }
    }
    
    protected virtual void OnLoop(){}
    
    void StopElevator(OnDapEvent e)
    {
        elevatorGoing = false;
    }
}