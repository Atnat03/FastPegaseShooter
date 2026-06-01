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
        }
    }
}