using UnityEngine;
using FMODUnity;

public class EnemyAudioController : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference idleSoundEvent;
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference deathEvent; // Tu przypisz dŸwiêk œmierci (Multi-Instrument)
    [SerializeField] private EventReference hitEvent;

    [Header("Idle Settings (Random)")]
    [SerializeField] private float minWaitTime = 5f;
    [SerializeField] private float maxWaitTime = 15f;

    [Header("Movement Settings")]
    [SerializeField] private float stepDistance = 2.0f;
    [SerializeField] private float minMoveSpeed = 0.1f;
    
    private float nextSoundTime;
    private float idleTimer;
    private float distanceTravelled;
    private Vector3 lastPosition;
    private bool isDead = false; // Flaga, ¿eby nie krzycza³ po œmierci

    void Start()
    {
        lastPosition = transform.position;
        ResetIdleTimer();
    }

    void Update()
    {
        if (isDead) return; // Jeœli trup, nie licz kroków ani idli

        HandleIdleSounds();
        HandleFootsteps();
    }

    public void PlayHitSound()
    {
        if (!hitEvent.IsNull && !isDead)
        {
            // Odtwarzamy dŸwiêk trafienia
            RuntimeManager.PlayOneShot(hitEvent, transform.position);
        }
    }

    public void PlayDeathSound()
    {
        if (isDead) return; // Zapobiega odtworzeniu dŸwiêku kilka razy
        isDead = true;

        if (!deathEvent.IsNull)
        {
            // PlayOneShot jest idealny, bo FMOD odpala dŸwiêk w œwiecie 
            // i "zapomina" o nim – zagra do koñca nawet po Destroy(gameObject)
            RuntimeManager.PlayOneShot(deathEvent, transform.position);
            Debug.Log("EnemyAudio: Odtworzono dŸwiêk œmierci.");
        }
    }

    void HandleIdleSounds()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= nextSoundTime)
        {
            PlayIdleSound();
            ResetIdleTimer();
        }
    }

    void HandleFootsteps()
    {
        Vector3 currentPos = transform.position;
        Vector3 movement = new Vector3(currentPos.x - lastPosition.x, 0, currentPos.z - lastPosition.z);
        float speed = movement.magnitude / Time.deltaTime;

        if (speed > minMoveSpeed)
        {
            distanceTravelled += movement.magnitude;
            if (distanceTravelled >= stepDistance)
            {
                PlayFootstep();
                distanceTravelled = 0f;
            }
        }
        lastPosition = currentPos;
    }

    public void PlayIdleSound()
    {
        if (!idleSoundEvent.IsNull && !isDead)
        {
            RuntimeManager.PlayOneShot(idleSoundEvent, transform.position);
        }
    }

    private void PlayFootstep()
    {
        if (!footstepEvent.IsNull && !isDead)
        {
            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
        }
    }

    void ResetIdleTimer()
    {
        idleTimer = 0f;
        nextSoundTime = Random.Range(minWaitTime, maxWaitTime);
    }
}