using System;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    public class DoorsTrigger : NetworkBusListener
    {
        [SerializeField] private int _index;
        Animator animator;

        public override void OnStartClient()
        {
            ListenToEvent<OnDoorOpen_TUTO>(OpenDoor);
        }

        private void OpenDoor(OnDoorOpen_TUTO data)
        {
            if (_index != data.indexDoor)
                return;

            Cons.Print("Open door");

            if (data.action == 0)
            {
                animator.SetTrigger("Open");
            }
            else
            {
                animator.SetTrigger("Close");
            }
        }
    }
}