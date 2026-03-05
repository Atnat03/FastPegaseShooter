using System;
using FishNet.Object;
using UnityEngine;

namespace LD
{
    public class CubeEnergyTest : MonoBehaviour
    {
        [SerializeField] EnergyManager _energyManager;
        [SerializeField] private EnergyDo _type;
        [SerializeField] private float value;
        
        
        enum EnergyDo
        {
            Add,
            Remove
        }
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerVisuelBridge>(out PlayerVisuelBridge player))
            {
                float v = _type == EnergyDo.Add ? value : -value;
                _energyManager.AddEnergy(v);
            }
        }
    }
}