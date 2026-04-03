using UnityEngine;

namespace generalScripts
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float _shakeDuration = 0f;
        private float _shakeIntensity = 0f;
        private Vector3 _originalPosition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            _originalPosition = transform.localPosition;
        }

        public void Shake(float duration = 0.05f, float intensity = 0.015f)
        {
            if (Time.timeScale == 0f) return;

            _shakeDuration = duration;
            _shakeIntensity = intensity;
        }

        private void LateUpdate()
        {
            if (_shakeDuration > 0)
            {
                transform.localPosition = _originalPosition + (Vector3)Random.insideUnitCircle * _shakeIntensity;
                _shakeDuration -= Time.unscaledDeltaTime;
            }
            else
            {
                _shakeDuration = 0f;
            }
        }
    }
}
