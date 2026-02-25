using UnityEngine;

public class DebugShootMark : MonoBehaviour
{
    public GameObject p_markPrefab;
    public Transform p_gunTransform;
    private GameObject _currentMark;

    public void Mark()
    {
        if (Physics.Raycast(p_gunTransform.position, p_gunTransform.forward, out RaycastHit hit))
        {
            if (_currentMark != null)
            {
                Destroy(_currentMark);
            }
            
            _currentMark = Instantiate(p_markPrefab);
        }
    }
}
