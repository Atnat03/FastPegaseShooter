using System.Collections;
using UnityEngine;

namespace Tuto
{
    public class Event_StartSpawn : BaseEvent
    {
        public override string DisplayName => "Start Spawn";

        [SerializeField] private int _indexSpawn;
        
        public override IEnumerator Execute()
        {
            manager.AskForStartSpawn(_indexSpawn);
            
            yield break;
        }
    }
}