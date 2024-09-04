using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// Handles player movement and jumping based on the input events.

public class PlayerController : MonoBehaviour
{
    [Header("Move Parameters")]
    public float moveSpeed = 10f;
    private Vector2 moveInput;
    private Vector2 lookInput;
    public float jumpStrength = 5f;

    [Header("Jump Parameters")]
    [SerializeField] public float jumpForce = 10.0f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private int xJumpCurrent = 0;
    [SerializeField] public int xJumpLimit = 1;

    [Header("Look Sensitivity")]
    [SerializeField] private Vector2 mouseSensitivity;
    [SerializeField] private Vector2 upDownRange;

    private CharacterController characterController;
    private Camera mainCamera;
    private InputHandler inputHandler;
    private Vector3 currentMovement;
    private float verticalRotation;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        inputHandler = GetComponent<InputHandler>();
    }
    private void OnEnable()
    {
        InputHandler.OnMovePerformed += OnMove;
        InputHandler.OnJumpPerformed += OnJump;
        InputHandler.OnLookPerformed += OnLook;
    }
    private void OnDisable()
    {
        InputHandler.OnMovePerformed -= OnMove;
        InputHandler.OnJumpPerformed -= OnJump;
        InputHandler.OnLookPerformed -= OnLook;
    }

    public void OnMove(Vector2 moveInp)
    {
        moveInput = moveInp;
    }

    public void OnLook(Vector2 lookInp)
    {
        lookInput = lookInp;
    }


    void HandleMove()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        worldDirection.Normalize();
        currentMovement.x = worldDirection.x * moveSpeed;
        currentMovement.z = worldDirection.z * moveSpeed;
        characterController.Move(currentMovement * Time.deltaTime);
        /*
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;
        Vector3 moveDirection = (forward + right).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        */
    }

    void HandleRotation()
    {
        float mouseX = lookInput.x * mouseSensitivity.x;
        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= lookInput.y * mouseSensitivity.y;
        verticalRotation = Mathf.Clamp(verticalRotation, upDownRange.y, upDownRange.x);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        /*
        mouseX = lookInput.x * mouseSensitivity.x;
        transform.Rotate(0, mouseX, 0);
        verticalRotation -= lookInput.y * mouseSensitivity.y;
        verticalRotation = Mathf.Clamp(verticalRotation, upDownRange.y, upDownRange.x);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        */
    }

    private void OnJump()
    {
        if (xJumpCurrent < xJumpLimit)  // inputHandler.JumpPressed && 
        {
            currentMovement.y = jumpStrength;
            xJumpCurrent++;
        }
    }

    private void HandleAss()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;
            xJumpCurrent = 0;
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
    }


    private void Update()
    {
        HandleMove();
        HandleRotation();
        HandleAss();

        // HandleJump();
        // characterController.Move(currentMovement * Time.deltaTime);

        // Sicherstellen, dass der Charakter auf der Plattform bleibt
        if (characterController.isGrounded)
        {
            // Finde die Plattform, auf der sich der Charakter befindet
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1f))
            {
                // Bewege den Charakter mit der Plattform
                transform.position = Vector3.MoveTowards(transform.position, hit.transform.position, moveSpeed * Time.fixedDeltaTime);
            }
        }
    }
}