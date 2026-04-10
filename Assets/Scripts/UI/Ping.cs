using TMPro;
using UnityEngine;

public class Ping : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textDistance;
    [SerializeField] private float _distanceMax = 100;
    [SerializeField] private Vector2 _scaleMap = new Vector2(1f, 10f);

    private Transform _target;
    private float _distance;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void LateUpdate()
    {
        if (_target != null)
        {
            Vector3 directionToCamera = _target.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
            
            _distance = Vector3.Distance(transform.position, _target.transform.position);
            
            _textDistance.text = _distance.ToString("F0") + "m";
            
            float normalizedDistance = Mathf.Clamp01(_distance / _distanceMax);
            transform.localScale = Vector3.one * Mathf.Lerp(_scaleMap.x, _scaleMap.y, normalizedDistance);
        }
    }
}
