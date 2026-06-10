using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilingScroller : MonoBehaviour
{
    [SerializeField] private List<GameObject> _tiles;
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private float _speed = 2f;

    private bool _running = false;

    void Start()
    {
        StartScroll();
    }
    
    public void StartScroll()
    {
        if (_running) return;
        _running = true;

        Vector3 axis = (_endPoint.position - _startPoint.position).normalized;
        float cycleLength = Vector3.Distance(_startPoint.position, _endPoint.position);

        foreach (var tile in _tiles)
            StartCoroutine(ScrollTile(tile, axis, cycleLength));
    }

    public void StopScroll()
    {
        _running = false;
        StopAllCoroutines();
    }

    private IEnumerator ScrollTile(GameObject tile, Vector3 axis, float cycleLength)
    {
        // Position de départ sur l'axe
        float initialProgress = Vector3.Dot(tile.transform.position - _startPoint.position, axis);
        // Offset perpendiculaire : fixe pour toujours
        Vector3 lateralOffset = tile.transform.position - (_startPoint.position + axis * initialProgress);

        float progress = initialProgress;

        while (_running)
        {
            progress += _speed * Time.deltaTime;

            // Wrap dans le cycle
            if (progress >= cycleLength) progress -= cycleLength;
            if (progress < 0f) progress += cycleLength;

            tile.transform.position = _startPoint.position + axis * progress + lateralOffset;

            yield return null;
        }
    }
}