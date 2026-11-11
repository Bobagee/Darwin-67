using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Velocidades de Movimiento")]
    [Tooltip("Velocidad normal al caminar (WASD)")]
    [SerializeField] private float walkSpeed = 4.0f;

    [Tooltip("Velocidad al mantener presionado Shift")]
    [SerializeField] private float sprintSpeed = 7.5f;

    [Tooltip("Fuerza de gravedad")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Controles y Resistencia")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private StaminaSistema staminaSistema;

    [Header("Cámara y Vista")]
    [SerializeField] private Transform playerCamera;
    [Tooltip("Sensibilidad recomendada: entre 2.0 y 5.0")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float topClamp = -80.0f;
    [SerializeField] private float bottomClamp = 80.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    public float CurrentSpeed { get; private set; }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (staminaSistema == null)
        {
            staminaSistema = GetComponent<StaminaSistema>();
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        ManejarVista();
        ManejarMovimiento();
    }

    private void ManejarVista()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void ManejarMovimiento()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        bool intentandoCorrer = Input.GetKey(sprintKey) && moveZ > 0.1f;
        bool tieneEstamina = staminaSistema == null || staminaSistema.CurrentStamina > 0f;
        bool estaCorriendo = intentandoCorrer && tieneEstamina;

        CurrentSpeed = estaCorriendo ? sprintSpeed : walkSpeed;

        controller.Move(move * CurrentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}