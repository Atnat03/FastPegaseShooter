using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(Collider))]
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private int _zoneId = 0;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerVisuelBridge bridge)) return;

            RespawnPointManager.Instance?.SetPlayerZoneAndPosition(bridge.OwnerId, _zoneId, transform.position);
            Debug.Log($"[SpawnZone] Joueur {bridge.OwnerId} -> zone {_zoneId} @ {transform.position}");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (TryGetComponent(out BoxCollider box))
                Gizmos.DrawCube(box.center, box.size);
            else if (TryGetComponent(out SphereCollider sphere))
                Gizmos.DrawSphere(sphere.center, sphere.radius);

            // Affiche la position de spawn
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
    }
}