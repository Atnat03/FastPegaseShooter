using UnityEngine;
using UnityEngine.InputSystem;

public class TirNul : MonoBehaviour
{
    #region Variables

    [Header("References")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _bulletPrefab;

    [Header("Settings")]
    [SerializeField] private float _fireRate = 0.2f;

    private float _nextFireTime;

    #endregion

    #region Unity Methods

    public void Start()
    {
        _playerCamera = Camera.main;
    }
    private void OnEnable()
    {
        _playerInput.actions["Shoot"].performed += Shoot;
    }

    private void OnDisable()
    {
        _playerInput.actions["Shoot"].performed -= Shoot;
    }

    #endregion

    #region Shooting Logic

    private void Shoot(InputAction.CallbackContext context)
    {
        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + _fireRate;

        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 direction = (targetPoint - _firePoint.position).normalized;

        Instantiate(_bulletPrefab, _firePoint.position, Quaternion.LookRotation(direction));
    }

    #endregion
}
