using generalScripts.Managers;
using UnityEngine;

namespace generalScripts
{

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
                cinemachineCamera = GetComponent<Behaviour>();
            }

            if (cinemachineCamera == null)
            {
                //Debug.LogWarning("[CameraTargetSetter] No Cinemachine component found!");
                return;
            }
            
            var type = cinemachineCamera.GetType();

            var followProp = type.GetProperty("Follow");
            var lookAtProp = type.GetProperty("LookAt");

            if (followProp != null) followProp.SetValue(cinemachineCamera, playerTransform);
            if (lookAtProp != null) lookAtProp.SetValue(cinemachineCamera, playerTransform);

            Debug.Log($"[CameraTargetSetter] Camera now following: {playerTransform.name}");
        }
    }
}
