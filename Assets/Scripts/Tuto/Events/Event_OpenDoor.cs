using System.Collections;

namespace Tuto
{
    public class Event_OpenDoor : BaseEvent
    {
        public override string DisplayName => "Open door";

        public enum Door {Open, Close}
        
        public Door actionToDo = Door.Close;
        public int doorIndex = 0; 
        
        public override IEnumerator Execute()
        {
            if (manager != null)
            {
                manager.AskForOpenDoorServerRpc(actionToDo, doorIndex);
            }
            
            yield break;
        }
    }
}