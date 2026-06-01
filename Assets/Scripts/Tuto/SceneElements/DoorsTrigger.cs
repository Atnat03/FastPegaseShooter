using System;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    public class DoorsTrigger : MonoBusListener
    {
        [SerializeField] private int _index;
        [SerializeField] Animator animator;

        //public override void OnStartClient()
        
        private void Awake()
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