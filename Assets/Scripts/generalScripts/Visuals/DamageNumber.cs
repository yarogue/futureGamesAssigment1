using UnityEngine;
using TMPro;

namespace generalScripts
{
    public class DamageNumber : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float floatSpeed = 1.5f;
        [SerializeField] private float fadeSpeed = 1f;
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private float randomSpreadX = 0.5f;

        private TextMeshPro _text;
        private Color _color;
        private float _timer;

        private void Awake()
        {
            _text = GetComponent<TextMeshPro>();
            if (_text == null)
            {
                _text = gameObject.AddComponent<TextMeshPro>();
            }
            
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize = 4f;
            _text.sortingOrder = 100;
        }

        public void Initialize(float damage, Color color)
        {
            _text.text = damage.ToString("F0");
            _text.color = color;
            _color = color;
            _timer = 0f;

            // Random horizontal offset for variety
            transform.position += new Vector3(Random.Range(-randomSpreadX, randomSpreadX), 0.3f, 0);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            
            // Float upward
            transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
            
            // Fade out
            _color.a = Mathf.Lerp(1f, 0f, _timer / lifetime);
            _text.color = _color;
            
            // Scale down slightly
            float scale = Mathf.Lerp(1f, 0.5f, _timer / lifetime);
            transform.localScale = Vector3.one * scale;

            if (_timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
