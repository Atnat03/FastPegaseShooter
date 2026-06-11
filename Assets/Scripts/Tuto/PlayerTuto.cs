using System;
using Controller;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    public class PlayerTuto : NetworkBusListener
    {
        [SerializeField] private GunBridgePlayer _gunPlayer;
        [SerializeField] private DroneThrower _dronePlayer;
        [SerializeField] private PlayerHealth _healPlayer;

        public Action<Capacity_TUTO, bool> OnUnlockCapa;

        public override void OnStartNetwork()
        {
            ListenToEvent<OnUnlockCapa_TUTO>(UnlockCapa);
        }

        private void Start()
        {
            OnUnlockCapa?.Invoke(Capacity_TUTO.ChargedShoot, _gunPlayer.p_unlockChargedShoot);
            OnUnlockCapa?.Invoke(Capacity_TUTO.Heal, _healPlayer.p_unlockCapa);
            OnUnlockCapa?.Invoke(Capacity_TUTO.Drone, _dronePlayer.p_unlockCapa);
        }

        public void UnlockCapa(OnUnlockCapa_TUTO data)
        {
            switch (data.capa)
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
            
            Cons.Print("Unlock capa");
            
            OnUnlockCapa?.Invoke(data.capa, true);
            InvokeEvent(new OnCapacityUnlocked { capacity = data.capa });
        }

        public bool IsCapaUnlock(Capacity_TUTO capa)
        {
            switch (capa)
            {
                case Capacity_TUTO.EnergyShoot:
                    return _gunPlayer.p_unlockSwapEnergyLaser;
                
                case Capacity_TUTO.ChargedShoot:
                    return _gunPlayer.p_unlockChargedShoot;
                
                case Capacity_TUTO.Drone:
                    return _dronePlayer.p_unlockCapa;
                
                case Capacity_TUTO.Heal:
                    return _healPlayer.p_unlockCapa;
            }

            return false;
        }
    }
}