using FishNet.Object;
using UnityEngine;

public class MiniMap : NetworkBehaviour
{
	[SerializeField] private Camera miniMapPrefab;
	private Camera _spawnedCam;
	
	[SerializeField] private SpriteRenderer miniMapSprite;

	public override void OnStartClient()
	{
		base.OnStartClient();

		if (!IsOwner)
		{
			miniMapPrefab.gameObject.SetActive(false);
			miniMapSprite.color = Color.yellow;
		}
		else
		{
			miniMapSprite.color = Color.cornflowerBlue;
		}
	}
}
