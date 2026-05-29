using System;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    public class DoorsTrigger : MonoBusListener
    {
        [SerializeField] private int _index;
        Animator animator;
        
        private void Awake()
        {
            ListenToEvent<OnDoorOpen_TUTO>(OpenDoor);
        }

        private void OpenDoor(OnDoorOpen_TUTO data)
        {
            if (_index != data.indexDoor)
                return;

            Cons.Print("Open door");
            
            animator.SetTrigger(data.action == Event_OpenDoor.Door.Open ? "Open" : "Close");
        }
    }
}