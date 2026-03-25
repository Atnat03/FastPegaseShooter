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
			miniMapSprite.color = Color.orange;
			return;
		}

		SpawnMiniMapCamera();
	}

	private void SpawnMiniMapCamera()
	{
		miniMapSprite.color = Color.deepSkyBlue;
		
		_spawnedCam = Instantiate(miniMapPrefab, transform, true);

		_spawnedCam.transform.position = transform.position + Vector3.up * 10f;
		_spawnedCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
	}
}
