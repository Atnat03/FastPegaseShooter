using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FakeGrapin : MonoBehaviour
{
    #region Variables

    [Header("References")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField]private Camera _playerCamera;

    #endregion
    
    private void OnEnable()
    {
        _playerInput.actions["Grapple"].performed += Grapple;
    }

    private void OnDisable()
    {
        _playerInput.actions["Grapple"].performed -= Grapple;
    }

    private void Grapple(InputAction.CallbackContext context)
    {
        Debug.Log("please");
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
            Debug.Log("je touche " + hit.transform.name);
            if (hit.transform.CompareTag("GrapplePoint2"))
            {
                newPos =  hit.transform.position + new Vector3(0, 2, 0);
                StartCoroutine(TPOverGrapin());
            }
        }
    }

    public Vector3 newPos;

    IEnumerator TPOverGrapin()
    {
        yield return new WaitForEndOfFrame();
        transform.position = newPos + new Vector3(0, 2, 0);
        yield return new WaitForEndOfFrame();
        transform.position = newPos + new Vector3(0, 2, 0);
        yield return new WaitForEndOfFrame();
        transform.position = newPos + new Vector3(0, 2, 0);
    }
}
