using UnityEngine;

public class TestBlendShape : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private float _speed = 2;

    void Update()
    {
        float value = (Mathf.Sin(Time.time * _speed) + 1) * 50;
        _skinnedMeshRenderer.SetBlendShapeWeight(0, value);
    }
}
