using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 50f;
    public float MouseSensitivity { get { return mouseSensitivity; } set { mouseSensitivity = value; } }
    [SerializeField] private Transform playerBody;

    [Header("Input References")]
    [SerializeField] private InputActionReference lookAction;

    private float xRotation = 0f;

    // Obsługa blokady pionowej i ociężałego sterowania w szarży
    private bool lockVerticalLook = false;
    private float turnSensitivityMultiplier = 1f;

    public void SetChargeLookState(bool isCharging, float turnMultiplier = 0.25f)
    {
        lockVerticalLook = isCharging;
        turnSensitivityMultiplier = isCharging ? Mathf.Clamp01(turnMultiplier) : 1f;

        if (isCharging)
        {
            // Płynne wyrównanie kamery do poziomu (zero w pionie)
            xRotation = 0f;
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
    }

    private void Update()
    {
        HandleLook();
    }

    private void HandleLook()
    {
        if (lookAction == null) return;

        Vector2 mouseDelta = lookAction.action.ReadValue<Vector2>();

        float mouseX = mouseDelta.x * (mouseSensitivity * turnSensitivityMultiplier) * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        if (!lockVerticalLook)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
