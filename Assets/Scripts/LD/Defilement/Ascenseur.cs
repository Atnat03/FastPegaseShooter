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
    protected float elapsed;
    private bool mustStop;

    void Start()
    {
        ListenToEvent<OnDapEvent>(e => StopElevator());
        ListenToEvent<OnDoorAtHeight>(StopElevatorRightNow);
        OnLoop();
    }
    

    public void StartDescente(Vector3 startPosition, Vector3 endPosition, float duration, float launchTime)
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
        _currentCoroutine = StartCoroutine(DescenteAscenseur(_localStart, localEnd, duration, launchTime, startElapsed));
    }

    private bool IsAtDoorHeight(float elapsed , float duration)
    {
        return (!_elevatorGoing && (elapsed / duration < .462 && elapsed / duration > .4615)) ;
    }

    private IEnumerator DescenteAscenseur(Vector3 localStart, Vector3 localEnd, float duration, float launchTime, float startElapsed)
    {
        //Toutes les tiles partent de la même base (=> le même temps initial)
        // on récupère le temps direct de AscenseurMAnager.cs
        while (true)
        {
            elapsed = (Time.time - launchTime + startElapsed) % duration;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(localStart, localEnd, elapsed / duration);

                if (!_elevatorGoing && (IsAtDoorHeight(elapsed, duration) || mustStop))
                {
                    InvokeEvent<OnDoorAtHeight>(new OnDoorAtHeight());
                    OnAscenseurStop(duration);
                    yield break;
                }

                yield return null;
                elapsed = (Time.time - launchTime + startElapsed) % duration;
            }

            transform.position = localStart;
            OnLoop();
        }
    }
    
    private void StopElevator()
    {
        _elevatorGoing = false;
    }
    
    private void StopElevatorRightNow(OnDoorAtHeight e) => mustStop = true;

    protected virtual void OnLoop() { }
    
    protected virtual void OnAscenseurStop(float duration) {}
}

public struct OnDoorAtHeight
{
    
}