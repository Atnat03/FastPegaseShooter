using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tuto
{
    public class FillAmountBridge : MonoBusListener
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private Image _image;

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

            targetFill = data.maxPercentage / 100f;
            speed = data.speed;
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