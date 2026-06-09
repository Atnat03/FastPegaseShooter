using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tuto
{
    public class Event_StartSpawn : BaseEvent
    {
        public override string DisplayName => "Start Spawn";

        [SerializeField] private List<int> _indexSpawn =  new List<int>();
        
        public override IEnumerator Execute()
        {
            manager.AskForStartSpawn(_indexSpawn);
            
            yield break;
        }
    }
}