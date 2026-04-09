using generalScripts.Managers;
using UnityEngine;

namespace generalScripts
{
    /// <summary>
    /// Attach this to the CinemachineCamera (or any camera-related) GameObject.
    /// It listens for GameSceneManager.OnPlayerSpawned and sets the Follow/LookAt targets.
    ///
    /// Works with both Cinemachine 2.x (CinemachineVirtualCamera)
    /// and Cinemachine 3.x (CinemachineCamera).
    /// Just drag whichever component is on your camera into the Inspector slot below.
    /// </summary>
    public class CameraTargetSetter : MonoBehaviour
    {
        [Tooltip("Drag the Cinemachine Virtual Camera component here (either version)")]
        [SerializeField] private Behaviour cinemachineCamera;

        private void OnEnable()
        {
            GameSceneManager.OnPlayerSpawned += SetTarget;
        }

        private void OnDisable()
        {
            GameSceneManager.OnPlayerSpawned -= SetTarget;
        }

        private void SetTarget(Transform playerTransform)
        {
            if (cinemachineCamera == null)
            {
                // Try to auto-find on this GameObject if not assigned
                cinemachineCamera = GetComponent<Behaviour>();
            }

            if (cinemachineCamera == null)
            {
                Debug.LogWarning("[CameraTargetSetter] No Cinemachine component found!");
                return;
            }

            // Use reflection so this compiles regardless of Cinemachine version
            var type = cinemachineCamera.GetType();

            var followProp = type.GetProperty("Follow");
            var lookAtProp = type.GetProperty("LookAt");

            if (followProp != null) followProp.SetValue(cinemachineCamera, playerTransform);
            if (lookAtProp != null) lookAtProp.SetValue(cinemachineCamera, playerTransform);

            Debug.Log($"[CameraTargetSetter] Camera now following: {playerTransform.name}");
        }
    }
}
