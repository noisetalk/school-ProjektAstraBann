using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public delegate void MoveAction(Vector2 moveInput);
    public static event MoveAction OnMovePerformed;
    
    public delegate void LookAction(Vector2 lookInput);
    public static event LookAction OnLookPerformed;

    public delegate void JumpAction();
    public static event JumpAction OnJumpPerformed;
    // public bool JumpPressed { get; private set; }
    // private bool jumpPressedLastFrame;
    
    public delegate void UseAction();
    public static event UseAction OnUsePerformed;
    
    public delegate void CastAction();
    public static event CastAction OnCastPerformed;
    
    public delegate void LinkAction();
    public static event LinkAction OnLinkPerformed;
    
    public delegate void PushAction();
    public static event PushAction OnPushPerformed;
    
    public delegate void PullAction();
    public static event PullAction OnPullPerformed;
    
    public delegate void RotateYAction();
    public static event RotateYAction OnRotateYPerformed;
    
    public delegate void RotateZAction();
    public static event RotateZAction OnRotateZPerformed;
    
    public delegate void EnlargeAction();
    public static event EnlargeAction OnEnlargePerformed;
    
    public delegate void ShrinkAction();
    public static event ShrinkAction OnShrinkPerformed;


    private InputActions playerInput;

    private void Awake()
    {
        playerInput = new InputActions();

        playerInput.Player.Move.performed += ctx => OnMovePerformed?.Invoke(ctx.ReadValue<Vector2>());
        playerInput.Player.Move.canceled += ctx => OnMovePerformed?.Invoke(ctx.ReadValue<Vector2>());
        playerInput.Player.Look.performed += ctx => OnLookPerformed?.Invoke(ctx.ReadValue<Vector2>());
        playerInput.Player.Look.canceled += ctx => OnLookPerformed?.Invoke(ctx.ReadValue<Vector2>());
        playerInput.Player.Jump.performed += _ => OnJumpPerformed?.Invoke();
        // playerInput.Player.Jump.performed += ctx => JumpPressed = true;
        // playerInput.Player.Jump.canceled += ctx => JumpPressed = false;
        playerInput.Player.Use.performed += ctx => OnUsePerformed?.Invoke();
        playerInput.Player.Cast.performed += ctx => OnCastPerformed?.Invoke();
        playerInput.Player.Link.performed += ctx => OnLinkPerformed?.Invoke();
        playerInput.Player.Push.performed += ctx => OnPushPerformed?.Invoke();
        playerInput.Player.Pull.performed += ctx => OnPullPerformed?.Invoke();
        playerInput.Player.RotateY.performed += ctx => OnRotateYPerformed?.Invoke();
        playerInput.Player.RotateZ.performed += ctx => OnRotateZPerformed?.Invoke();
        playerInput.Player.Enlarge.performed += ctx => OnEnlargePerformed?.Invoke();
        playerInput.Player.Shrink.performed += ctx => OnShrinkPerformed?.Invoke();
        
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
    }
}
