using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveActionRef;
    [SerializeField] private InputActionReference attackActionRef;
    [SerializeField] private InputActionReference rollActionRef;
    [SerializeField] private InputActionReference shieldActionRef;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Input variables
    private Vector2 moveInput;
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction rollAction;
    private InputAction shieldAction;

    // Components
    private PlayerStateManager stateManager;
    private PlayerAnimationController animController;
    private PlayerMovement movement;

    private void Awake()
    {
        // Get components
        stateManager = GetComponent<PlayerStateManager>();
        animController = GetComponent<PlayerAnimationController>();
        movement = GetComponent<PlayerMovement>();

        // Setup input actions
        if (moveActionRef != null)
            moveAction = moveActionRef.action;
        if (attackActionRef != null)
            attackAction = attackActionRef.action;
        if (rollActionRef != null)
            rollAction = rollActionRef.action;
        if (shieldActionRef != null)
            shieldAction = shieldActionRef.action;
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.Enable();
        if (attackAction != null)
        {
            attackAction.Enable();
            attackAction.performed += OnAttackPerformed;
        }
        if (rollAction != null)
        {
            rollAction.Enable();
            rollAction.performed += OnRollPerformed;
        }
        if (shieldAction != null)
        {
            shieldAction.Enable();
            shieldAction.performed += OnShieldPressed;
            shieldAction.canceled += OnShieldReleased;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.Disable();
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
            attackAction.Disable();
        }
        if (rollAction != null)
        {
            rollAction.performed -= OnRollPerformed;
            rollAction.Disable();
        }
        if (shieldAction != null)
        {
            shieldAction.performed -= OnShieldPressed;
            shieldAction.canceled -= OnShieldReleased;
            shieldAction.Disable();
        }
    }

    private void Start()
    {
        if (animController != null)
            animController.PlayAnimation(PlayerAnimationController.ANIM_IDLE_DOWN);
    }

    private void Update()
    {
        if (IsDead) return;
        
        HandleInput();
        HandleMovement();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        movement.HandleMovement(moveInput);
    }

    private void HandleInput()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
    }

    private void HandleMovement()
    {
        bool isMoving = moveInput.magnitude > 0.1f && !stateManager.IsInSpecialState();
        Vector2 direction = isMoving ? moveInput.normalized : stateManager.lastDirection;
        
        stateManager.SetMoving(isMoving, direction);
    }

    private void HandleAnimation()
    {
        if (stateManager.IsInSpecialState()) return;

        string animationToPlay = animController.GetAnimationName(stateManager.lastDirection, stateManager.isMoving);

        if (animationToPlay != animController.CurrentAnimation)
        {
            animController.PlayAnimation(animationToPlay);
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        stateManager.QueueAttack();
    }

    private void OnRollPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 rollDir = moveInput.magnitude > 0.1f ? moveInput.normalized : stateManager.lastDirection;
        
        // Get animation duration and start roll with movement
        animController.GetAnimationDurationAsync(animController.GetRollingAnimationName(rollDir), (duration) => {
            movement.StartRoll(rollDir, duration);
        });
        
        stateManager.StartRoll(rollDir);
    }

    private void OnShieldPressed(InputAction.CallbackContext ctx)
    {
        stateManager.StartShield();
    }

    private void OnShieldReleased(InputAction.CallbackContext ctx)
    {
        stateManager.StopShield();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed) OnAttackPerformed(context);
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed) OnRollPerformed(context);
    }

    public void OnShield(InputAction.CallbackContext context)
    {
        if (context.performed) OnShieldPressed(context);
        else if (context.canceled) OnShieldReleased(context);
    }

    public void TakeDamage(Vector2 damageDirection, float damage = 1f)
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        
        if (!health.IsDead)
        {
            stateManager.TakeDamage(damageDirection);
        }
    }

    // Public properties
    public bool IsMoving => stateManager.isMoving;
    public bool IsAttacking => stateManager.isAttacking;
    public bool IsRolling => stateManager.isRolling;
    public bool IsShielding => stateManager.isShielding;
    public bool IsInvulnerable => stateManager.isInvulnerable;
    public bool IsTakingDamage => stateManager.isTakingDamage;
    public Vector2 MoveDirection => stateManager.lastDirection;
    public float MoveSpeed => movement.MoveSpeed;
    public bool IsDead => GetComponent<PlayerHealth>()?.IsDead ?? false;

    public void SetMoveSpeed(float newSpeed) => movement.SetMoveSpeed(newSpeed);
    public bool CanTakeDamageFromDirection(Vector2 damageDirection) => stateManager.CanTakeDamageFromDirection(damageDirection);
}