using UnityEngine;

public class PlayerEnergizedStateView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private PlayerEnergizedState _playerEnergizedState;

	[Header("UI")] 
	[SerializeField] private GameObject _uiEnergized;

	#endregion


	#region Fonctions

	void OnEnable()
	{
		_playerEnergizedState.OnEnergized += UpdateUI;
	}

	void OnDisable()
	{
		_playerEnergizedState.OnEnergized += UpdateUI;
	}
	
	private void UpdateUI(bool isActive)
	{
		_uiEnergized.SetActive(isActive);
	}

	#endregion
}
