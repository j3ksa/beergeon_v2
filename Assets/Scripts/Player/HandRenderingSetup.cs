using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Beergeon.Player
{
    /// <summary>
    /// Automatically configures the Main Camera and a child "Hand Camera" 
    /// to use URP Camera Stacking for the weapon/hand.
    /// This ensures the hand is drawn on top of everything and doesn't clip into walls.
    /// </summary>
    [System.Serializable]
    public struct SpriteSettings
    {
        public Sprite sprite;
        public Vector3 rotationOffset;
        public Vector3 positionOffset; // Might be useful too, adding for completeness
    }

    /// <summary>
    /// Automatically configures the Main Camera and a child "Hand Camera" 
    /// to use URP Camera Stacking for the weapon/hand.
    /// This ensures the hand is drawn on top of everything and doesn't clip into walls.
    /// Also handles automatic scaling and rotation based on the current sprite.
    /// </summary>
    public class HandRenderingSetup : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The Layer name for the Player Hand. Must exist in Project Settings > Tags and Layers.")]
        [SerializeField] private string handLayerName = "PlayerHand";
        
        [Tooltip("Field of View for the Hand Camera. Usually wider than main FOV to make weapon look good.")]
        [SerializeField] private float handFOV = 60f;

        [Tooltip("Target height in world units for the hand sprite. If > 0, the hand will auto-scale to this height.")]
        [SerializeField] private float targetSpriteHeight = 1.0f;

        [Tooltip("Specific settings for individual sprites (like rotation for attacks).")]
        [SerializeField] private System.Collections.Generic.List<SpriteSettings> spriteSettings;

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;
        [SerializeField] private GameObject handObject;

        private SpriteRenderer handRenderer;
        private Sprite lastSprite;
        private Vector3 initialLocalPosition;
        
        // Public property to allow other scripts (like PlayerHand) to apply temporary rotations
        [HideInInspector] public Quaternion animatedRotation = Quaternion.identity;
        [HideInInspector] public Vector3 animatedPosition = Vector3.zero;

        private void Awake()
        {
            if (handObject != null)
            {
                initialLocalPosition = handObject.transform.localPosition;
            }
        }

        public Vector3 CurrentSpritePositionOffset { get; private set; } = Vector3.zero;

        private void Start()
        {
            SetupCameras();
            SetupHandLayer();
            DisableHandCollision();

            // Cache renderer for auto-scaling
            if (handObject != null)
            {
                handRenderer = handObject.GetComponentInChildren<SpriteRenderer>();
                if (initialLocalPosition == Vector3.zero)
                {
                    initialLocalPosition = handObject.transform.localPosition;
                }
                UpdateHandVisuals();
            }
        }

        private void LateUpdate()
        {
            // Update every frame to support live-tweaking in Inspector
            // and to ensure rotation/scale persists 
            if (handRenderer != null)
            {
                UpdateHandVisuals();
            }
        }

        private void UpdateHandVisuals()
        {
            if (handRenderer == null || handRenderer.sprite == null) return;

            lastSprite = handRenderer.sprite;
            
            // --- 1. Rotation & Position Offsets ---
            Vector3 targetRotation = Vector3.zero;
            Vector3 targetPosOffset = Vector3.zero;

            if (spriteSettings != null)
            {
                foreach (var setting in spriteSettings)
                {
                    if (setting.sprite == lastSprite)
                    {
                        targetRotation = setting.rotationOffset;
                        targetPosOffset = setting.positionOffset; 
                        break;
                    }
                }
            }

            CurrentSpritePositionOffset = targetPosOffset;

            // Apply rotation
            handObject.transform.localRotation = Quaternion.Euler(targetRotation) * animatedRotation;
            
            // Apply position only if PlayerHand is not handling it directly
            PlayerHand playerHand = GetComponentInChildren<PlayerHand>();
            if (playerHand == null) playerHand = GetComponentInParent<PlayerHand>();
            if (playerHand == null && handObject != null)
            {
                handObject.transform.localPosition = initialLocalPosition + targetPosOffset + animatedPosition;
            }

            // --- 2. Scale ---
            if (targetSpriteHeight <= 0) return;

            // Calculate current height in world units (without our custom scaling)
            // sprite.bounds.size is in local space (pixels / ppu)
            // We want to set localScale so that: localScale.y * sprite.bounds.size.y == targetSpriteHeight
            
            float spriteHeight = handRenderer.sprite.bounds.size.y;
            if (spriteHeight > 0.001f)
            {
                float itemScale = (playerHand != null && playerHand.CurrentItem != null) ? playerHand.CurrentItem.handScaleMultiplier : 1.0f;
                float newScale = (targetSpriteHeight / spriteHeight) * itemScale;
                // Apply uniform scaling to preserve aspect ratio
                handObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }

        [ContextMenu("Setup Now")]
        public void SetupCameras()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            
            // 1. Validate Main Camera
            if (mainCamera == null)
            {
                
                return;
            }

            var mainCamData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if (mainCamData == null)
            {
                mainCamData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            // Ensure Main Camera is Base
            mainCamData.renderType = CameraRenderType.Base;

            // 2. Setup/Create Hand Camera
            if (handCamera == null)
            {
                // Look for existing child camera first
                var existingCam = mainCamera.transform.Find("HandCamera");
                if (existingCam != null)
                {
                    handCamera = existingCam.GetComponent<Camera>();
                }
                else
                {
                    // Create new
                    GameObject camObj = new GameObject("HandCamera");
                    camObj.transform.SetParent(mainCamera.transform);
                    camObj.transform.localPosition = Vector3.zero;
                    camObj.transform.localRotation = Quaternion.identity;
                    handCamera = camObj.AddComponent<Camera>();
                }
            }

            // Configure Hand Camera
            var handCamData = handCamera.GetComponent<UniversalAdditionalCameraData>();
            if (handCamData == null)
            {
                handCamData = handCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            handCamData.renderType = CameraRenderType.Overlay;
            handCamera.clearFlags = CameraClearFlags.Depth; // Important for Overlay
            handCamera.fieldOfView = handFOV;
            handCamera.depth = 1; // Higher than main

            // 3. Layer Management
            int handLayer = LayerMask.NameToLayer(handLayerName);
            if (handLayer == -1)
            {
                
                return;
            }

            // Main Camera should NOT see the hand
            mainCamera.cullingMask = mainCamera.cullingMask & ~(1 << handLayer);

            // Hand Camera should ONLY see the hand
            handCamera.cullingMask = 1 << handLayer;

            // 4. Add to Stack
            bool alreadyInStack = false;
            foreach (var cam in mainCamData.cameraStack)
            {
                if (cam == handCamera)
                {
                    alreadyInStack = true;
                    break;
                }
            }

            if (!alreadyInStack)
            {
                mainCamData.cameraStack.Add(handCamera);
                
            }
        }

        private void SetupHandLayer()
        {
            if (handObject != null)
            {
                int layer = LayerMask.NameToLayer(handLayerName);
                if (layer != -1)
                {
                    SetLayerRecursive(handObject, layer);
                }
            }
        }

        private void DisableHandCollision()
        {
            if (handObject != null)
            {
                // Disable any colliders directly on the hand representation
                // We do NOT want to disable the CharacterController on the parent player
                var colliders = handObject.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    // Only disable non-trigger colliders that might be interfering, 
                    // or all of them if we want purely visual hand.
                    // For now, let's disable them to avoid "collide with everything".
                    if (!col.isTrigger) 
                    {
                        col.enabled = false;
                    }
                }
            }
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}
