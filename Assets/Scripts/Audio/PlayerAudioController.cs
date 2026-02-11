using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.InputSystem; // Wymagane dla Keyboard.current

public class PlayerAudioController : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference jumpLandEvent;

    [Header("Settings")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private float stepDistance = 2.5f;

    // --- TUTAJ S¥ BRAKUJ¥CE ZMIENNE (DEKLARACJE) ---
    private EventInstance jumpInstance;
    private float distanceTravelled;
    private bool isGroundedPrevious = true;

    void Update()
    {
        // Sprawdzamy czy controller nie jest pusty, ¿eby unikn¹æ kolejnych b³êdów
        if (controller == null) return;

        HandleFootsteps();
        HandleJumpAndLand();

        isGroundedPrevious = controller.isGrounded;
    }

    void HandleFootsteps()
    {
        // DODAJEMY WARUNEK: Jeœli gracz nie jest na ziemi, resetujemy licznik i wychodzimy
        if (!controller.isGrounded)
        {
            distanceTravelled = 0f;
            return;
        }

        // Dodatkowo: blokujemy kroki, jeœli w tej samej klatce gracz nacisn¹³ skok
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            distanceTravelled = 0f;
            return;
        }

        // Standardowa logika kroków
        if (controller.velocity.magnitude > 0.1f)
        {
            Vector3 flatVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            distanceTravelled += flatVelocity.magnitude * Time.deltaTime;

            if (distanceTravelled >= stepDistance)
            {
                RuntimeManager.PlayOneShot(footstepEvent, transform.position);
                distanceTravelled = 0f;
            }
        }
    }

    void HandleJumpAndLand()
    {
        // 1. WYBICIE (New Input System)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            jumpInstance = RuntimeManager.CreateInstance(jumpLandEvent);
            jumpInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            jumpInstance.start();
        }

        // 2. L¥DOWANIE
        if (controller.isGrounded && !isGroundedPrevious)
        {
            if (jumpInstance.isValid())
            {
                jumpInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                jumpInstance.keyOff();
                jumpInstance.release();
            }
        }
    }

    void OnDestroy()
    {
        if (jumpInstance.isValid())
        {
            jumpInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            jumpInstance.release();
        }
    }
}