using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Beergeon.Player
{
    /// <summary>
    /// Automatically configures the Main Camera and a child "Hand Camera" 
    /// to use URP Camera Stacking for the weapon/hand.
    /// This ensures the hand is drawn on top of everything and doesn't clip into walls.
    /// </summary>
    public class HandRenderingSetup : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The Layer name for the Player Hand. Must exist in Project Settings > Tags and Layers.")]
        [SerializeField] private string handLayerName = "PlayerHand";
        
        [Tooltip("Field of View for the Hand Camera. Usually wider than main FOV to make weapon look good.")]
        [SerializeField] private float handFOV = 60f;

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;
        [SerializeField] private GameObject handObject;

        private void Start()
        {
            SetupCameras();
            SetupHandLayer();
            DisableHandCollision();
        }

        [ContextMenu("Setup Now")]
        public void SetupCameras()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            
            // 1. Validate Main Camera
            if (mainCamera == null)
            {
                Debug.LogError("[HandRenderingSetup] No Main Camera found!");
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
                Debug.LogError($"[HandRenderingSetup] Layer '{handLayerName}' does not exist! Please add it in Project Settings.");
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
                Debug.Log("[HandRenderingSetup] Added HandCamera to Main Camera stack.");
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
