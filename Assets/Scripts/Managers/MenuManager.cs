using System.Collections;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GameObject capchat;
	[SerializeField] private GameObject characters;
	[SerializeField] private CanvasGroup fade;
	[SerializeField] public int indexSceneGame = 0;
	
	#endregion


	#region Fonctions

	public void ClickOnPlay()
	{
		StartCoroutine(FadeOut());
	}

	IEnumerator FadeOut()
	{
		float duration = 1;
		float t = 0;
		
		fade.alpha = 0;

		while (t < duration)
		{
			t += Time.deltaTime;
			
			fade.alpha = t / duration;
			
			yield return null;
		}
		
		fade.alpha = 1;
		characters.SetActive(false);
		
		capchat.SetActive(true);
	}
	
	#endregion
}
