using UnityEngine;

public class DestroyOnLoad : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private float _timeToDestroy = 1;
	
	#endregion


	#region Fonctions

	void Awake()
	{
		Destroy(gameObject, _timeToDestroy);
	}
	
	#endregion
}
