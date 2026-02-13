using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerAudioController : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference jumpLandEvent;

    [Header("Movement Settings")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private float stepDistance = 2.5f;
    [SerializeField] private float minVelocity = 0.5f;

    private EventInstance jumpInstance;
    private float distanceTravelled;
    private bool isGroundedPrevious = true;
    private float lastVelocityY;

    void Update()
    {
        if (controller == null) return;

        HandleFootsteps();
        HandleJumpAndLandDetection();

        isGroundedPrevious = controller.isGrounded;
        lastVelocityY = controller.velocity.y;
    }

    void HandleFootsteps()
    {
        if (!controller.isGrounded)
        {
            distanceTravelled = 0f;
            return;
        }

        if (controller.velocity.magnitude > minVelocity)
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

    void HandleJumpAndLandDetection()
    {
        // 1. WYKRYWANIE WYBICIA (Skok)
        // Jeœli w poprzedniej klatce byliœmy na ziemi, a teraz mamy du¿¹ prêdkoœæ w górê
        if (isGroundedPrevious && !controller.isGrounded && controller.velocity.y > 0.1f)
        {
            // Debug.Log("AUDIO: Wykryto start skoku przez fizykê!");
            StartJumpAudio();
        }

        // 2. WYKRYWANIE L¥DOWANIA
        if (controller.isGrounded && !isGroundedPrevious)
        {
            // Debug.Log("AUDIO: Wykryto kontakt z ziemi¹!");
            FinishJumpAudio();
        }
    }

    private void StartJumpAudio()
    {
        if (jumpInstance.isValid())
        {
            jumpInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            jumpInstance.release();
        }

        jumpInstance = RuntimeManager.CreateInstance(jumpLandEvent);
        jumpInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        jumpInstance.start();
    }

    private void FinishJumpAudio()
    {
        if (jumpInstance.isValid())
        {
            jumpInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            jumpInstance.keyOff();
            jumpInstance.release();
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