using System;
using System.Collections;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    [Serializable]
    public struct NotificationData
    {
        public string text;
        public NotificationTarget target;
        public NotificationDisableAction disableAction;
        public float duration;
    }
    
    public class Event_Notification : BaseEvent
    {
        public override string DisplayName => "Notification";

        [TextArea(2, 5)]
        public string notificationText;

        public NotificationTarget target;
        public NotificationDisableAction disableAction;
        public float duration = 1;

        public NotificationData GetData()
        {
            return new NotificationData
            {
                text = notificationText,
                target = target,
                disableAction = disableAction,
                duration = duration
            };
        }

        public override IEnumerator Execute()
        {
            Cons.Print("NOTIFICATION");
            
            manager.AskForNotification(GetData());

            yield break;
        }
    }
}