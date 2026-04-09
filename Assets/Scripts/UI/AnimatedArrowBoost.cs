using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimatedArrowBoost : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _duration = 2f;
    [SerializeField] private float _yValue = 100f;
    [SerializeField] private Vector2 _timeBeforeStart = new Vector2(0, 1);

    private void Awake()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
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
            
            Vector2 startPos = _rectTransform.anchoredPosition;

            while (elapsedTime < _duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _duration;
                
                float y = Mathf.Lerp(0, _yValue, progress);
                _rectTransform.anchoredPosition = startPos + Vector2.up * y;
                
                _canvasGroup.alpha = Mathf.Sin(progress * Mathf.PI);
                
                yield return null;
            }

            _canvasGroup.alpha = 0;
            _rectTransform.anchoredPosition = startPos;
        }
    }
}