using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    [SerializeField] private Transform _door;
    [SerializeField] private Transform _doorTarget;
    [SerializeField] private int _playerNeeded = 1;
    [SerializeField] private bool mustClose = false;
    [SerializeField] private float _openingTime;

    private List<GameObject> playersDetected = new List<GameObject>();
    private Collider _doorCollider;
    private Vector3 _doorDefaultPos;
    private Quaternion _doorDefaultRot;

    void Start()
    {
        _doorCollider = _door.GetComponent<Collider>();
        _doorDefaultPos = _door.position;
        _doorDefaultRot = _door.rotation;
    }
    
    public void DetectPlayer(GameObject player)
    {
        if (!playersDetected.Contains(player))
        {
            playersDetected.Add(player);
            if (playersDetected.Count >= _playerNeeded) OpenDoor();
        }
    }

    public void PlayerLeave(GameObject player)
    {
        if (playersDetected.Contains(player))
        {
            playersDetected.Remove(player);
            if (mustClose)
            {
                if(playersDetected.Count < _playerNeeded)CloseDoor();
            }
        }
    }

    private void OpenDoor()
    {
        _doorCollider.enabled = false;
        StopAllCoroutines();
        StartCoroutine(OpenDoorCoroutine());
    }

    IEnumerator OpenDoorCoroutine()
    {
        float elapsedTime = 0;
        while (elapsedTime < _openingTime)
        {
            elapsedTime += Time.deltaTime;
            _door.position = Vector3.Lerp(_door.position, _doorTarget.position, elapsedTime / _openingTime);
            _door.rotation =  Quaternion.Lerp(_door.rotation, _doorTarget.rotation, elapsedTime / _openingTime);
            yield return null;
        }
    }

    private void CloseDoor()
    {
        _doorCollider.enabled = true;
        StopAllCoroutines();
        StartCoroutine(CloseDoorCoroutine());
    }
    
    IEnumerator CloseDoorCoroutine()
    {
        float elapsedTime = 0;
        while (elapsedTime < _openingTime)
        {
            elapsedTime += Time.deltaTime;
            _door.position = Vector3.Lerp(_door.position, _doorDefaultPos, elapsedTime / _openingTime);
            _door.rotation =  Quaternion.Lerp(_door.rotation, _doorDefaultRot, elapsedTime / _openingTime);
            yield return null;
        }
    }

}