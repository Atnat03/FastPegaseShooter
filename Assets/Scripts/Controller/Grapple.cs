using System.Collections;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grapple : NetworkBehaviour
{
    #region Variables
    
    [Header("References")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Rigidbody rb;
    
    [Header("To tweak")]
    [SerializeField] private float _castWidth = .5f;
    [SerializeField] private float _castMaxDistance = 100f;
    [SerializeField] private float _grapplingSpeed;
    [SerializeField] private float _endGrappleImpulseForce = 3f;
    
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
        if (Physics.SphereCast(_camTransform.position, _castWidth, _camTransform.forward, out RaycastHit hit, _castMaxDistance,LayerMask.GetMask("Default"),QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag("GrapplePoint"))
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
            rb.MovePosition(Vector3.MoveTowards(transform.position, _currentGrapplePoint.position, _grapplingSpeed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }

        if (playerInput.actions["Grapple"].IsPressed())
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * _endGrappleImpulseForce, ForceMode.Impulse);
        }

        _currentGrapplePoint = null;
    }
}
