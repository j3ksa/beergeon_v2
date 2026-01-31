using UnityEngine;
using FMODUnity;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("FMOD Settings")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private string surfaceParameterName = "Surface";

    [Header("Movement Settings")]
    [Tooltip("How many meters the player moves before a sound plays")]
    [SerializeField] private float stepDistance = 2.0f;
    [SerializeField] private float raycastDistance = 1.5f;

    private CharacterController _controller;
    private float _distanceTravelled;

    void Start()
    {
        _controller = GetComponent<CharacterController>();

        // Safety check to ensure a CharacterController exists
        if (_controller == null)
        {
            Debug.LogError("PlayerFootsteps script requires a CharacterController on the same GameObject.");
        }
    }

    void Update()
    {
        // Only track distance if the player is touching the ground and actually moving
        if (_controller.isGrounded && _controller.velocity.magnitude > 0.1f)
        {
            // We use horizontal velocity (X and Z) so jumping doesn't trigger "steps" in the air
            Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0, _controller.velocity.z);
            _distanceTravelled += horizontalVelocity.magnitude * Time.deltaTime;

            if (_distanceTravelled >= stepDistance)
            {
                PlayFootstep();
                _distanceTravelled = 0f;
            }
        }
    }

    void PlayFootstep()
    {
        // Create the FMOD instance
        FMOD.Studio.EventInstance footstep = RuntimeManager.CreateInstance(footstepEvent);

        // Check what surface we are walking on
        float surfaceValue = DetermineSurface();
        footstep.setParameterByName(surfaceParameterName, surfaceValue);

        // Attach sound to the player's 3D position
        footstep.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        // Start and immediately release the instance (FMOD handles cleanup)
        footstep.start();
        footstep.release();
    }

    private float DetermineSurface()
    {
        RaycastHit hit;
        // Shoots a ray down from the player's center
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            // Match these tags to the tags you've created in Unity
            switch (hit.collider.tag)
            {
                case "Stone": return 0f;
                case "Metal": return 1f;
                case "Water": return 2f;
                case "Value5": return 3f;
                default: return 0f; // Default surface
            }
        }
        return 0f;
    }
}