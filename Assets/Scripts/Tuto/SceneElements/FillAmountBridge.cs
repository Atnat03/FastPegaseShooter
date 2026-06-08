using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tuto
{
    public class FillAmountBridge : MonoBusListener
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private Image _image;
        
        [Header("Animation Settings")]
        [SerializeField] private Vector2 _maxScale = new Vector2(1.2f, 1.2f);
        [SerializeField] private float _scaleSpeed = 2f;
        [SerializeField] private float _durationScale = 1f;

        [Header("Shake Settings")]
        [SerializeField] private float _angleJiggle = 5f;
        [SerializeField] private float _shakeSpeed = 10f;
        [SerializeField] private float _durationJiggle = 1f;

        private float targetFill;
        private float speed;

        private Coroutine currentAnimation;
        private Vector3 defaultScale;
        private Quaternion defaultRotation;

        private void Awake()
        {
            ListenToEvent<OnFillAmount_TUTO>(UpdateFillAmount);

            defaultScale = _ui.transform.localScale;
            defaultRotation = _ui.transform.localRotation;

            _ui.SetActive(false);
        }

        private void UpdateFillAmount(OnFillAmount_TUTO data)
        {
            _ui.SetActive(data.activated);

            if (!data.activated)
            {
                StopCurrentAnimation();
                return;
            }

            targetFill = data.maxPercentage / 100f;
            speed = data.speed;

            StopCurrentAnimation();

            switch (data.type)
            {
                case AnimationBar.Scale:
                    currentAnimation = StartCoroutine(ScaleAnimation());
                    break;

                case AnimationBar.Vibration:
                    currentAnimation = StartCoroutine(VibrationAnimation());
                    break;
            }
        }

        private void StopCurrentAnimation()
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            _ui.transform.localScale = defaultScale;
            _ui.transform.localRotation = defaultRotation;
        }

        private IEnumerator ScaleAnimation()
        {
            float t = 0;
            
            Vector3 startScale = _ui.transform.localScale;
            
            while (t < _durationScale)
            {
                t += Time.deltaTime;
                
                float scale = 1f + Mathf.Sin(Time.time * _scaleSpeed) * (_maxScale.x - 1f);

                _ui.transform.localScale = new Vector3(
                    scale,
                    scale,
                    1f
                );

                yield return null;
            }

            _ui.transform.localScale = startScale;
        }

        private IEnumerator VibrationAnimation()
        {
            float t = 0;

            Quaternion startRotation = _ui.transform.localRotation;
            
            while (t < _durationJiggle)
            {
                t += Time.deltaTime;
                
                float angle = Mathf.Sin(Time.time * _shakeSpeed) * _angleJiggle;

                _ui.transform.localRotation = Quaternion.Euler(0, 0, angle);

                yield return null;
            }
            
            _ui.transform.localRotation = startRotation; 
        }

        private void Update()
        {
            _image.fillAmount = Mathf.Lerp(
                _image.fillAmount,
                targetFill,
                Time.deltaTime * speed
            );
        }
    }
}