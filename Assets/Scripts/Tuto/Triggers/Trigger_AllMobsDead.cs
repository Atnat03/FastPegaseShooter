using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tuto.Triggers
{
    [Serializable]
    public class Trigger_AllMobsDead : BaseTrigger
    {
        public override string DisplayName => "All Enemy Dead";

        public List<int> spawnZoneIndices = new();

        private List<SpawnZoneTutorial> _spawnZones = new();
        private HashSet<SpawnZoneTutorial> _completedZones = new();

        public void InjectSpawnZones(List<SpawnZoneTutorial> allZones)
        {
            _spawnZones = spawnZoneIndices
                .Where(i => i >= 0 && i < allZones.Count)
                .Select(i => allZones[i])
                .ToList();
        }

        public override void Initialize(TutoManager tuto) { }

        public override void Activate()
        {
            _completedZones.Clear();

            if (_spawnZones == null || _spawnZones.Count == 0) return;

            foreach (SpawnZoneTutorial zone in _spawnZones)
            {
                if (zone != null)
                    zone.p_onSpawnZoneComplete += OnZoneComplete;
            }
        }

        public override void Dispose()
        {
            foreach (SpawnZoneTutorial zone in _spawnZones)
            {
                if (zone != null)
                    zone.p_onSpawnZoneComplete -= OnZoneComplete;
            }
        }

        private void OnZoneComplete(SpawnZoneTutorial zone)
        {
            _completedZones.Add(zone);
            
            if (_completedZones.Count >= _spawnZones.Count)
                OnActivated?.Invoke();
        }
    }
}