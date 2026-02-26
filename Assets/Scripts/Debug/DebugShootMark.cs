using UnityEngine;

public class DebugShootMark : MonoBehaviour
{
    public GameObject p_markPrefab;
    private GameObject _currentMark;
    [SerializeField] private Shoot shoot;

    void OnEnable()
    {
        shoot.p_shootingAction += Mark;
    }

    void OnDisable()
    {
        shoot.p_shootingAction -= Mark;
    }
    
    private void Mark()
    {
        if (Physics.Raycast(shoot.p_visualWeapon.transform.position + shoot.p_visualWeapon.transform.forward * .3f, shoot.p_visualWeapon.transform.forward, out RaycastHit hit, LayerMask.GetMask("Owner")))
        {
            if (_currentMark != null)
            {
                Destroy(_currentMark);
            }
            
            _currentMark = Instantiate(p_markPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
        }
    }
}
