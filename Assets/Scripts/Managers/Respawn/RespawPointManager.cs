using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class RespawnPointManager : MonoBehaviour
    {
        public static RespawnPointManager Instance { get; private set; }

        // PlayerId -> (zoneId, position)
        private readonly Dictionary<int, (int zoneId, Vector3 position)> _playerData = new();

        [SerializeField] private Vector3 _defaultPosition = new Vector3(30, 0, -23.5f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("[RespawnPointManager] Initialisé");
        }

        public void SetPlayerZoneAndPosition(int playerId, int zoneId, Vector3 position)
        {
            _playerData[playerId] = (zoneId, position);
            Debug.Log($"[RespawnPointManager] Joueur {playerId} -> zone {zoneId} @ {position}");
        }

        public Vector3 GetRespawnPosition(int playerId)
        {
            if (_playerData.TryGetValue(playerId, out var data))
            {
                Debug.Log($"[RespawnPointManager] ✅ Joueur {playerId} respawn @ {data.position}");
                return data.position;
            }

            Debug.LogWarning($"[RespawnPointManager] ⚠️ Joueur {playerId} sans zone connue -> position par défaut {_defaultPosition}");
            return _defaultPosition;
        }
    }
}