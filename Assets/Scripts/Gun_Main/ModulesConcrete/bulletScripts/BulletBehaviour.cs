using FishNet.Object;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    [HideInInspector] public float p_damage;
    [HideInInspector] public float p_speed;

    [HideInInspector] public GameObject p_markPrefab;
    private GameObject _currentMark;
    private bool _hasHit = false;

    private void FixedUpdate()
    {
        DetectCollision();
        Move();
    }

    private void Move()
    {
        transform.Translate(transform.forward * (p_speed * Time.deltaTime), Space.World);
    }

    private void DetectCollision()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                p_speed * Time.fixedDeltaTime, ~LayerMask.NameToLayer("Owner"), QueryTriggerInteraction.Ignore))
        {
            _hasHit = true;
            
            Destroy(Instantiate(p_markPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal)), 3f);
            Destroy(gameObject);
        }
    }

    private void HitTarget(RaycastHit hit)
    {
        Destroy(Instantiate(p_markPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal)), 3f);
        if (hit.collider.TryGetComponent<IDamagable>(out IDamagable iDamagable))
        {
            iDamagable.TakeDamage((int)p_damage);
        }
        Destroy(gameObject);
    }
}