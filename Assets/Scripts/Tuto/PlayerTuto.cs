using System;
using Controller;
using UnityEngine;

namespace Tuto
{
    public class PlayerTuto : NetworkBusListener
    {
        [SerializeField] private GunBridgePlayer _gunPlayer;
        [SerializeField] private DroneThrower _dronePlayer;
        [SerializeField] private PlayerHealth _healPlayer;

        public Action<Capacity_TUTO, bool> OnUnlockCapa;

        private void Start()
        {
            OnUnlockCapa?.Invoke(Capacity_TUTO.ChargedShoot, _gunPlayer.p_unlockChargedShoot);
            OnUnlockCapa?.Invoke(Capacity_TUTO.Heal, _healPlayer.p_unlockCapa);
            OnUnlockCapa?.Invoke(Capacity_TUTO.Drone, _dronePlayer.p_unlockCapa);
        }

        public void UnlockCapa(Capacity_TUTO capacity)
        {
            switch (capacity)
            {
                case Capacity_TUTO.EnergyShoot:
                    _gunPlayer.p_unlockSwapEnergyLaser = true;
                    break;
                case Capacity_TUTO.ChargedShoot:
                    _gunPlayer.p_unlockChargedShoot = true;
                    break;
                case Capacity_TUTO.Drone:
                    _dronePlayer.p_unlockCapa = true;
                    break;
                case Capacity_TUTO.Heal:
                    _healPlayer.p_unlockCapa = true;
                    break;
            }
            
            OnUnlockCapa?.Invoke(capacity, true);
        }
    }
}