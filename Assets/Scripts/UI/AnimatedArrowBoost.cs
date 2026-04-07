using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimatedArrowBoost : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField]CanvasGroup _canvasGroup;
	[SerializeField] private float _duration = 2f;
	[SerializeField] private float _yValue = 100f;
	[SerializeField] private Vector2 _timeBeforeStart = new Vector2(0, 1);
	private float elapsedTime = 0;

	#endregion


	#region Fonctions

	private void OnEnable()
	{
		elapsedTime = 0;
		_canvasGroup.alpha = 0;
		StartCoroutine(Animation());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator Animation()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(_timeBeforeStart.x, _timeBeforeStart.y));

			float elapsedTime = 0;
			_canvasGroup.alpha = 0;
			Vector3 startPos = transform.position;

			while (elapsedTime < _duration)
			{
				elapsedTime += Time.deltaTime;
				
				float y = Mathf.Lerp(0, _yValue, elapsedTime / _duration);
				
				transform.position += Vector3.up * y;

				_canvasGroup.alpha = Mathf.PingPong(elapsedTime / _duration, _duration);
				
				yield return null;
			}

			_canvasGroup.alpha = 0;
			transform.position = startPos;
		}
	}

	#endregion
}
