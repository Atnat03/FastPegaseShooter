using System.Collections;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grapple : NetworkBehaviour
{
    #region Variables
    
    [Header("References")]
    [SerializeField] PlayerInput playerInput;
    
    [Header("To tweak")]
    [SerializeField] private float _castWidth = 3f;
    
    Transform _camTransform;
    private Transform _currentGrapplePoint ;
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();
        _camTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        playerInput.actions["Grapple"].performed += CastGrapple;
    }

    void OnDisable()
    {
        playerInput.actions["Grapple"].performed -= CastGrapple;
    }
    
    void CastGrapple(InputAction.CallbackContext ctx)
    {
        if (Physics.SphereCast(_camTransform.position, _castWidth, _camTransform.forward, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                _currentGrapplePoint =  hit.collider.transform;
                StartCoroutine(GrappleCoroutine());
            }
        }
    }

    IEnumerator GrappleCoroutine()
    {
        while (Vector3.Distance(transform.position, _currentGrapplePoint.position) > 0.1f &&
               playerInput.actions["Grapple"].IsPressed())
        {
            yield return new WaitForFixedUpdate();
        }

        _currentGrapplePoint = null;
    }
}
