using System;
using System.Collections;
using UnityEngine;

public class Ascenseur : MonoBehaviour
{
	AscenseurManager _manager;
	private Collider _collider;
	
	#region Fonctions

	private void Awake()
	{
		_collider = GetComponent<Collider>();
	}

	public void StartDescente(
		Vector3 startPosition, 
		Vector3 endPosition, 
		float duration,
		AscenseurManager manager)
	{
		_manager = manager;
		
		StartCoroutine(DescenteAscenseur(startPosition, endPosition, duration));
	}

	private IEnumerator DescenteAscenseur(Vector3 startPosition, Vector3 endPosition, float duration)
	{
		float elapsedTime = 0;
		
		transform.position = startPosition;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			
			transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
			
			yield return null;
		}
		
		transform.position = endPosition;
		
		Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_manager != null)
		{
			if (other.TryGetComponent(out PlayerVisuelBridge player))
			{
				_manager.SpawnNewAscenseur();
				_collider.enabled = false;
			}
		}
	}

	#endregion
}
