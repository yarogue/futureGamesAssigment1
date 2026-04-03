using System;
using generalScripts;
using generalScripts.Interfaces;
using UnityEngine;

namespace MainCharacterScripts
{
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("Component References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerShooting playerShooting;
        [SerializeField] private PlayerInputProcessor inputProcessor;

        [Header("Stats")]
        [SerializeField] private PlayerStats playerStats;

        [Header("Rotation Settings")]
        [Tooltip("Controls how fast the aim sprite rotates towards the mouse (Sensitivity)")]
        [SerializeField] private float aimRotationSpeed = 8f;

        [Tooltip("Rotation speed multiplier for the body (delayed follow)")]
        [SerializeField] private float bodyRotationSpeed = 3f;

        [Tooltip("Additional rotation from A/D steering")]
        [SerializeField] private float steeringRotationSpeed = 100f;

        [Header("Visuals")]
        [SerializeField] private Transform lookDirectionSprite;
        [SerializeField] private generalScripts.Visuals.SpriteParallaxSystem parallaxSystem;

        private void Awake()
        {
            ServiceLocator.RegisterService<IPlayerController>(this);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterService<IPlayerController>(this);
        }

        public void Start()
        {
            playerMovement.Initialize(playerStats);
            
            // Reset missiles for new game (ScriptableObject.OnEnable only runs once)
            if (playerStats != null)
            {
                playerStats.currentMissileAmount = playerStats.maxMissileAmount;
            }

            if (inputProcessor != null) return;
            inputProcessor = GetComponent<PlayerInputProcessor>();
            if (inputProcessor == null)
            {
                //Debug.LogError("[PlayerController] PlayerInputProcessor component not found!");
            }

            if (parallaxSystem == null)
            {
                parallaxSystem = GetComponentInChildren<generalScripts.Visuals.SpriteParallaxSystem>();
            }
        }

        private void Update()
        {
            if (inputProcessor == null) return;

            HandleBoost();
            HandleMovement();
            HandleRotation();
            HandleSteering();
        }

        private void HandleMovement()
        {
            var movementDirection = Vector2.zero;

            if (inputProcessor.CanMove && inputProcessor.ThrottleInput != 0)
            {
                movementDirection = (Vector2)transform.up * inputProcessor.ThrottleInput;

                if (movementDirection.magnitude > 1f)
                {
                    movementDirection.Normalize();
                }
            }

            playerMovement.SetInput(movementDirection);
            playerMovement.Tick();

            if (parallaxSystem != null)
            {
                parallaxSystem.UpdateParallax(movementDirection);
            }
        }

        private void HandleBoost()
        {
            if (inputProcessor != null && playerMovement != null)
            {
                playerMovement.SetBoosting(inputProcessor.IsBoosting);
            }
        }

        private void HandleRotation()
        {
            if (!inputProcessor.ShouldRotate) return;
            var targetAngle = inputProcessor.TargetRotationAngle;
            var smoothFactor = inputProcessor.GetRotationSmoothFactor();
            var currentBodyAngle = transform.eulerAngles.z;
            var targetBodyAngle = (lookDirectionSprite != null) ? lookDirectionSprite.eulerAngles.z : targetAngle;
            var newBodyAngle = Mathf.LerpAngle(
                currentBodyAngle,
                targetBodyAngle,
                bodyRotationSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Euler(0, 0, newBodyAngle);
            if (lookDirectionSprite == null) return;
            var currentSpriteAngle = lookDirectionSprite.eulerAngles.z;
            var newSpriteAngle = Mathf.LerpAngle(
                currentSpriteAngle,
                targetAngle,
                aimRotationSpeed * smoothFactor * Time.deltaTime
            );
            lookDirectionSprite.rotation = Quaternion.Euler(0, 0, newSpriteAngle);
        }

        private void HandleSteering()
        {
            /*
            if (inputProcessor.SteerInput != 0)
            {
                transform.Rotate(
                    Vector3.forward,
                    inputProcessor.SteerInput * steeringRotationSpeed * Time.deltaTime
                );
            }
            */
        }
    }
}