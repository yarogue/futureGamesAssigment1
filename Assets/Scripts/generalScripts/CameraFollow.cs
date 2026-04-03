using UnityEngine;

namespace generalScripts
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        private Transform _target;

        private void Start()
        {
            FindPlayer();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                FindPlayer();
                return;
            }

            var desiredPosition = _target.position + offset;
            var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

        private void FindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
        }
    }
}
