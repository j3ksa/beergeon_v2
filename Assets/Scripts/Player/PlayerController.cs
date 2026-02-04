using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Input References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference interactAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInteraction = GetComponent<PlayerInteraction>();
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
        bool fallbackInteract = (interactAction == null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);

        if (fallbackInteract || (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame))
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
        
        // Drop Item
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.DropActiveItem(transform.position + transform.forward + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
        velocity.y += gravity * Time.deltaTime;
        //...

        Vector3 move = Vector3.zero;
        if (moveAction != null)
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();
            move = transform.right * input.x + transform.forward * input.y;
        }

        controller.Move((move * moveSpeed + Vector3.up * velocity.y) * Time.deltaTime);
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
