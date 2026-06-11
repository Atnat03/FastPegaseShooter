using System;
using UnityEngine;

public class TriggerAscenceur : MonoBusListener
{
	#region Variables
	
	[SerializeField] Animator[] _animatorToTrigger;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		TriggerAnimators(false);
	}
	
	//debug
	[ContextMenu("TriggerAscenseur")]
	void Trigger()
	{
		TriggerAnimators(true);
				
		InvokeEvent(new OnAscenseurStart());
				
		enabled = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			TriggerAnimators(true);
				
			InvokeEvent(new OnAscenseurStart());
				
			enabled = false;
		}
	}
	
	private void TriggerAnimators(bool state)
	{
		foreach (Animator animator in _animatorToTrigger)
		{
			animator.enabled = state;
		}
	}
	
	#endregion
}
