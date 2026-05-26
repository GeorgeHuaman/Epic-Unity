using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    public CharacterController controller;
    public Transform playerCamera;

    public float speed = 5f;
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private bool canMove = true;
    private bool canLook = true;

    private InputAction moveAction;
    private InputAction lookAction;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Ocultar y bloquear el cursor del mouse al centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;

        // Configurar las acciones del Input System (Project-wide actions)
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
    }

    void Update()
    {
        if (moveAction == null || lookAction == null) return;

        // 1. Mirar con el Mouse (Delta)
        if (canLook)
        {
            Vector2 lookValue = lookAction.ReadValue<Vector2>();
            float mouseX = lookValue.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookValue.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        // 2. Caminar con WASD
        if (canMove)
        {
            Vector2 moveValue = moveAction.ReadValue<Vector2>();
            float x = moveValue.x;
            float z = moveValue.y;

            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);
        }

        // Gravedad básica para que no flote
        controller.Move(Vector3.down * 9.8f * Time.deltaTime);
    }

    // Función para bloquear el movimiento cuando está leyendo o respondiendo un quiz
    public void SetMovement(bool state)
    {
        canMove = state;
        canLook = state;
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    public void SetCursorState(bool locked)
    {
        canLook = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
