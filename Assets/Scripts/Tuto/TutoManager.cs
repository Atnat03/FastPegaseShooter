using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;

namespace Tuto
{
	public class TutoManager : NetworkBusListener
	{
		[SerializeField] private ScenarioSO _scenarioSequence;

		public override void OnStartNetwork()
		{
			StartTuto();
		}

		void StartTuto()
		{
			StartCoroutine(Tutoriel());
		}

		IEnumerator Tutoriel()
		{
			foreach (Scenario scenario in _scenarioSequence._scenarioList)
			{
				
			}
		}
	}
}