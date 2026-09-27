using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [Header("Input References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference interactAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private PlayerInteraction playerInteraction;
    private PlayerStamina playerStamina;
    private Vector3 spawnPosition;

    // Tymczasowe spowolnienie (np. po potężnym ataku ciężką bronią)
    private float temporarySpeedMultiplier = 1f;
    private float temporarySlowTimer = 0f;

    public void ApplyTemporarySlow(float multiplier, float duration)
    {
        temporarySpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        temporarySlowTimer = Mathf.Max(temporarySlowTimer, duration);
    }

    // Mnożnik prędkości od trzymanej broni (np. Sztylet +25%)
    private float weaponSpeedModifier = 1f;

    public float WeaponSpeedModifier
    {
        get => weaponSpeedModifier;
        set => weaponSpeedModifier = Mathf.Max(0.1f, value);
    }

    // Mnożnik prędkości od założonej zbroi (np. ciężka metalowa zbroja spowalnia)
    private float armorSpeedModifier = 1f;

    public float ArmorSpeedModifier
    {
        get => armorSpeedModifier;
        set => armorSpeedModifier = Mathf.Clamp(value, 0.1f, 2f);
    }

    [Header("Dash / Dodge Settings")]
    [Tooltip("Prędkość uniku (dasha).")]
    [SerializeField] private float dashSpeed = 13.5f;
    [Tooltip("Czas trwania uniku w sekundach.")]
    [SerializeField] private float dashDuration = 0.18f;
    [Tooltip("Czas odnowienia (cooldown) uniku w sekundach.")]
    [SerializeField] private float dashCooldown = 0.5f;
    [Tooltip("Koszt staminy na wykonanie uniku.")]
    [SerializeField] private float dashStaminaCost = 8f;
    [Tooltip("Czy gracz unika obrażeń podczas dasha (ramki nietykalności i-frames).")]
    [SerializeField] private bool giveIFramesDuringDash = true;

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    public bool IsDashing => dashTimer > 0f;
    public bool HasIFrames => giveIFramesDuringDash && dashTimer > 0f;
    public float DashCooldownRemaining => Mathf.Max(0f, dashCooldownTimer);

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInteraction = GetComponent<PlayerInteraction>();
        playerStamina = GetComponent<PlayerStamina>();
    }

    private void Start()
    {
        if (moveAction == null) Debug.LogError("[PlayerController] 'Move Action' is not assigned!");
        if (jumpAction == null) Debug.LogError("[PlayerController] 'Jump Action' is not assigned!");

        if (interactAction == null)
        {
            Debug.LogWarning("[PlayerController] 'Interact Action' is NOT assigned! Creating temporary fallback on 'E' key.");
            // Create a runtime binding for 'E'
            var fallbackAction = new InputAction(name: "Interact", binding: "<Keyboard>/e");
            // Wrap it in an InputActionReference (requires creating a scriptable object at runtime is hard, so we just use the raw action local workaround)
            // Actually, we can just assign a new InputActionReference if we wanted, but let's just use a local workaround
            // Since we rely on 'interactAction' which is a Reference, we can't easily inject a raw Action into it if the field is Reference.
            // However, we can create a temporary method to handle fallback input in Update.
        }
        else
        {
            Debug.Log($"[PlayerController] Interact Action assigned: {interactAction.action.name} (Enabled: {interactAction.action.enabled})");
        }

        spawnPosition = transform.position;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();

        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();

        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    // Callback method
    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log($"[PlayerController] Interact Event Fired! Phase: {context.phase}");
        if (playerInteraction != null)
        {
            playerInteraction.PerformInteraction();
        }
        else
        {
            // Try to find it again just in case (e.g. added at runtime)
            playerInteraction = GetComponent<PlayerInteraction>();
            if (playerInteraction != null) playerInteraction.PerformInteraction();
            else Debug.LogError("[PlayerController] PlayerInteraction component MISSING.");
        }
    }

    private void Update()
    {
        if (Praktyki.EQ.MinecraftInventoryUI.IsOpen) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (jumpAction != null && jumpAction.action.triggered && isGrounded)
        {
            Debug.Log("Jump Triggered!");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Direct Keyboard Check
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (playerInteraction != null) playerInteraction.PerformInteraction();
        }

        // Mouse Scroll for Inventory
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();
            if (scroll != 0 && Inventory.instance != null)
            {
                Inventory.instance.ScrollActiveSlot(scroll);
            }
        }

        // Hotbar Number Keys 1-9
        if (Keyboard.current != null && Inventory.instance != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(2);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(3);
            else if (Keyboard.current.digit5Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(4);
            else if (Keyboard.current.digit6Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(5);
            else if (Keyboard.current.digit7Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(6);
            else if (Keyboard.current.digit8Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(7);
            else if (Keyboard.current.digit9Key.wasPressedThisFrame) Inventory.instance.SetActiveSlot(8);
        }
        
        // Drop Item
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Inventory.instance != null)
            {
                Vector3 forward = transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f) forward.Normalize();
                else forward = transform.forward;

                float dropDist = 1.0f;
                if (Physics.Raycast(transform.position, forward, out RaycastHit wallHit, 1.2f, ~0, QueryTriggerInteraction.Ignore))
                {
                    dropDist = Mathf.Max(0.35f, wallHit.distance - 0.25f);
                }

                Vector3 horizontalPos = transform.position + forward * dropDist;
                Vector3 rayOrigin = horizontalPos + Vector3.up * 0.2f;
                Vector3 dropPos;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, 3.0f, ~0, QueryTriggerInteraction.Ignore))
                {
                    dropPos = groundHit.point + Vector3.up * 0.45f;
                }
                else
                {
                    dropPos = horizontalPos - Vector3.up * 0.55f;
                }

                Quaternion dropRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                Inventory.instance.DropActiveItem(dropPos, dropRot);
            }
        }
        velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime, -30f);
        //...

        Vector3 move = Vector3.zero;
        bool isMoving = false;
        if (moveAction != null)
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();
            move = transform.right * input.x + transform.forward * input.y;
            isMoving = input.sqrMagnitude > 0.01f;
        }

        // Obsługa uniku / dasha (L Ctrl)
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        bool ctrlPressed = Keyboard.current != null && (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame);
        if (ctrlPressed && dashCooldownTimer <= 0f && dashTimer <= 0f)
        {
            TryPerformDash(move, isMoving);
        }

        // Obsługa tymczasowego spowolnienia
        if (temporarySlowTimer > 0f)
        {
            temporarySlowTimer -= Time.deltaTime;
            if (temporarySlowTimer <= 0f)
            {
                temporarySpeedMultiplier = 1f;
            }
        }

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            controller.Move((dashDirection * dashSpeed + Vector3.up * velocity.y) * Time.deltaTime);
        }
        else
        {
            float currentSpeed = moveSpeed * temporarySpeedMultiplier * weaponSpeedModifier * armorSpeedModifier;
            if (playerStamina != null && playerStamina.UpdateSprintState(isMoving))
            {
                currentSpeed *= playerStamina.SprintSpeedMultiplier;
            }

            controller.Move((move * currentSpeed + Vector3.up * velocity.y) * Time.deltaTime);
        }

        // Zabezpieczenie przed wypadnięciem pod mapę
        if (transform.position.y < -20f)
        {
            Respawn();
        }
    }

    private void TryPerformDash(Vector3 currentMoveDir, bool isMoving)
    {
        // Sprawdź stan staminy
        if (playerStamina != null)
        {
            if (playerStamina.CurrentStamina < dashStaminaCost)
            {
                return; // Brak wymaganej staminy na unik
            }

            playerStamina.DrainStamina(dashStaminaCost);
        }

        // Kierunek dasha: w stronę wciskanego klawisza ruchu (WASD) lub w przód patrzenia, jeśli gracz stoi w miejscu
        if (isMoving && currentMoveDir.sqrMagnitude > 0.001f)
        {
            dashDirection = currentMoveDir.normalized;
        }
        else
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            dashDirection = forward.sqrMagnitude > 0.001f ? forward.normalized : transform.forward;
        }

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        Debug.Log($"<color=#00FFFF><b>[Unik]</b></color> Wykonano zwinny unik (L-Ctrl)! Pozostała stamina: {(playerStamina != null ? playerStamina.CurrentStamina : 0):F1}");
    }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPosition;
        velocity = Vector3.zero;
        controller.enabled = true;
        Debug.LogWarning($"[PlayerController] Gracz wypadł poza mapę! Zrespawnowano na pozycji: {spawnPosition}");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Handle ICollidable interactions
        ICollidable collidable = hit.gameObject.GetComponent<ICollidable>();
        if (collidable != null)
        {
            collidable.OnCollide(gameObject);
        }

        // Basic Rigidbody pushing
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        // Don't push objects below us
        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.linearVelocity = pushDir * moveSpeed; // Simple push based on move speed
    }

    private void OnTriggerEnter(Collider other)
    {
        ITriggerable triggerable = other.GetComponent<ITriggerable>();
        if (triggerable != null)
        {
            triggerable.OnTrigger(gameObject);
        }
    }
}
