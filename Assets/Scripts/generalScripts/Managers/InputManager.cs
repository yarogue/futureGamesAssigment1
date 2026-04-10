using System;
using generalScripts.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using generalScripts.Interfaces;

namespace generalScripts.Managers
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        private PlayerInputs playerInputs;

        // Input Values
        public float ThrottleInput { get; private set; }
        public float SteerInput { get; private set; }
        public Vector2 AimPosition { get; private set; }

        // Button States
        public bool IsAttacking { get; private set; }
        public bool IsSpecialAttacking { get; private set; }
        public bool IsBoosting { get; private set; }

        // Events
        public event Action OnAttackPressed;
        public event Action OnAttackReleased;
        public event Action OnSpecialAttackPressed;
        public event Action OnSpecialAttackReleased;
        public event Action OnPausePressed;
        public event Action OnBoostPressed;
        public event Action OnBoostReleased;

        // Platform Detection
        public bool IsMobile
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                    return true;
#elif UNITY_WEBGL
                    return Application.isMobilePlatform;
#else
                return false;
#endif
            }
        }

        public string CurrentControlScheme => "Keyboard&Mouse";


        private void Awake()
        {
            playerInputs = new PlayerInputs();

            SubscribeToInputEvents();

            EnableGameplayControls();

            ServiceLocator.RegisterService<IInputManager>(this);
            //Debug.Log("[InputManager] Registered with ServiceLocator");
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputEvents();
            playerInputs?.Dispose();

            ServiceLocator.UnregisterService<IInputManager>(this);
            //Debug.Log("[InputManager] Unregistered from ServiceLocator");
        }

        private void Update()
        {
            // Read input values every frame
            if (playerInputs.Gameplay.enabled)
            {
                ThrottleInput = playerInputs.Gameplay.Throttle.ReadValue<float>();
                SteerInput = playerInputs.Gameplay.Steer.ReadValue<float>();
                AimPosition = playerInputs.Gameplay.Aim.ReadValue<Vector2>();

                IsAttacking = playerInputs.Gameplay.Attack.IsPressed();
                IsSpecialAttacking = playerInputs.Gameplay.SpecialAttack.IsPressed();
                IsBoosting = playerInputs.Gameplay.Boost.IsPressed();
            }
        }

        private void SubscribeToInputEvents()
        {
            // Attack
            playerInputs.Gameplay.Attack.started += ctx => OnAttackPressed?.Invoke();
            playerInputs.Gameplay.Attack.canceled += ctx => OnAttackReleased?.Invoke();

            // Boost
            playerInputs.Gameplay.Boost.started += ctx => OnBoostPressed?.Invoke();
            playerInputs.Gameplay.Boost.canceled += ctx => OnBoostReleased?.Invoke();

            // Special Attack
            playerInputs.Gameplay.SpecialAttack.started += ctx => OnSpecialAttackPressed?.Invoke();
            playerInputs.Gameplay.SpecialAttack.canceled += ctx => OnSpecialAttackReleased?.Invoke();

            // Pause
            playerInputs.Gameplay.Pause.performed += ctx => OnPausePressed?.Invoke();
        }

        private void UnsubscribeFromInputEvents()
        {
            if (playerInputs == null) return;

            playerInputs.Gameplay.Attack.started -= ctx => OnAttackPressed?.Invoke();
            playerInputs.Gameplay.Attack.canceled -= ctx => OnAttackReleased?.Invoke();

            playerInputs.Gameplay.SpecialAttack.started -= ctx => OnSpecialAttackPressed?.Invoke();
            playerInputs.Gameplay.SpecialAttack.canceled -= ctx => OnSpecialAttackReleased?.Invoke();

            playerInputs.Gameplay.Pause.performed -= ctx => OnPausePressed?.Invoke();

            playerInputs.Gameplay.Boost.started -= ctx => OnBoostPressed?.Invoke();
            playerInputs.Gameplay.Boost.canceled -= ctx => OnBoostReleased?.Invoke();
        }

        public void EnableGameplayControls()
        {
            playerInputs.Gameplay.Enable();
            playerInputs.UI.Disable();
            //Debug.Log("[InputManager] Gameplay controls enabled");
        }

        public void DisableGameplayControls()
        {
            playerInputs.Gameplay.Disable();
            //Debug.Log("[InputManager] Gameplay controls disabled");
        }

        public void EnableUIControls()
        {
            playerInputs.UI.Enable();
            playerInputs.Gameplay.Disable();
            //Debug.Log("[InputManager] UI controls enabled");
        }

        public void DisableUIControls()
        {
            playerInputs.UI.Disable();
            //Debug.Log("[InputManager] UI controls disabled");
        }

        public void StartRebinding(string actionName, int bindingIndex, Action<bool> onComplete)
        {
            var action = playerInputs.asset.FindAction(actionName);
            if (action == null)
            {
                //Debug.LogError($"[InputManager] Action '{actionName}' not found");
                onComplete?.Invoke(false);
                return;
            }

            //Debug.Log($"[InputManager] Starting rebind for {actionName}");

            var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse/position")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op =>
                {
                    //Debug.Log($"[InputManager] Rebind complete for {actionName}");
                    SaveBindings();
                    onComplete?.Invoke(true);
                    op.Dispose();
                })
                .OnCancel(op =>
                {
                    //Debug.Log($"[InputManager] Rebind cancelled for {actionName}");
                    onComplete?.Invoke(false);
                    op.Dispose();
                })
                .Start();
        }

        public void ResetBindingsToDefault()
        {
            playerInputs.asset.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey("InputBindings");
            //Debug.Log("[InputManager] Reset all bindings to default");
        }

        public void SaveBindings()
        {
            var rebinds = playerInputs.asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("InputBindings", rebinds);
            PlayerPrefs.Save();
            //Debug.Log("[InputManager] Bindings saved");
        }

        public void LoadBindings()
        {
            var rebinds = PlayerPrefs.GetString("InputBindings", string.Empty);
            if (!string.IsNullOrEmpty(rebinds))
            {
                playerInputs.asset.LoadBindingOverridesFromJson(rebinds);
                //Debug.Log("[InputManager] Bindings loaded");
            }
        }
    }
}
