using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tuto
{
    [CreateAssetMenu(fileName = "New Scenario", menuName = "Tutoriel/ScenarioSO")]
    public class ScenarioSO : ScriptableObject
    {
        public List<Scenario> _scenarioList = new List<Scenario>();
    }

    [Serializable]
    public class Scenario
    {
        public string ScenarioName;
        [SerializeReference] public BaseTrigger trigger;
        [SerializeReference] public List<BaseEvent> eventsList = new List<BaseEvent>();
    }
}