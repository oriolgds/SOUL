using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveActionRef;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    // Input variables
    private Vector2 moveInput;
    private bool isMoving;
    private InputAction moveAction;

    // Direction tracking
    private Vector2 lastDirection = Vector2.down; // Default facing down
    private string currentAnimation = "";

    // Animation parameter names
    private const string ANIM_IDLE_DOWN = "idle_down";
    private const string ANIM_IDLE_UP = "idle_up";
    private const string ANIM_IDLE_LEFT = "idle_left";
    private const string ANIM_IDLE_RIGHT = "idle_right";
    private const string ANIM_IDLE_LEFT_DOWN = "idle_left_down";
    private const string ANIM_IDLE_RIGHT_DOWN = "idle_right_down";
    private const string ANIM_IDLE_LEFT_UP = "idle_left_up";
    private const string ANIM_IDLE_RIGHT_UP = "idle_right_up";

    private const string ANIM_RUN_DOWN = "run_down";
    private const string ANIM_RUN_UP = "run_up";
    private const string ANIM_RUN_LEFT = "run_left";
    private const string ANIM_RUN_RIGHT = "run_right";
    private const string ANIM_RUN_LEFT_DOWN = "run_left_down";
    private const string ANIM_RUN_RIGHT_DOWN = "run_right_down";
    private const string ANIM_RUN_LEFT_UP = "run_left_up";
    private const string ANIM_RUN_RIGHT_UP = "run_right_up";

    private void Awake()
    {
        // Get components if not assigned
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        // Setup input action
        if (moveActionRef != null)
        {
            moveAction = moveActionRef.action;
        }
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.Enable();
            Debug.Log("Move action enabled");
        }
        else
        {
            Debug.LogWarning("Move action is null! Please assign the Move Input Action Reference in the inspector.");
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }

    private void Start()
    {
        // Set initial animation
        PlayAnimation(ANIM_IDLE_DOWN);

        // Debug component check
        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        if (animator == null) Debug.LogError("Animator not found!");
        if (moveAction == null) Debug.LogError("Move action not configured!");
    }

    private void Update()
    {
        HandleInput();
        HandleMovement();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        // Apply movement
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    private void HandleInput()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();

            // Debug input
            if (moveInput.magnitude > 0.1f)
            {
                Debug.Log($"Input detected: {moveInput}, magnitude: {moveInput.magnitude}");
            }
        }
    }

    private void HandleMovement()
    {
        // Check if player is moving
        isMoving = moveInput.magnitude > 0.1f;

        // Update last direction only when moving
        if (isMoving)
        {
            lastDirection = moveInput.normalized;
            Debug.Log($"Moving in direction: {lastDirection}");
        }
    }

    private void HandleAnimation()
    {
        string animationToPlay = GetAnimationName(lastDirection, isMoving);

        // Only change animation if it's different
        if (animationToPlay != currentAnimation)
        {
            PlayAnimation(animationToPlay);
            currentAnimation = animationToPlay;
        }
    }

    private string GetAnimationName(Vector2 direction, bool moving)
    {
        // Normalize direction for comparison
        direction = direction.normalized;

        // Define angle thresholds for 8-direction movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Normalize angle to 0-360 range
        if (angle < 0) angle += 360;

        // Determine direction based on angle
        if (angle >= 337.5f || angle < 22.5f) // Right
        {
            return moving ? ANIM_RUN_RIGHT : ANIM_IDLE_RIGHT;
        }
        else if (angle >= 22.5f && angle < 67.5f) // Right-Up
        {
            return moving ? ANIM_RUN_RIGHT_UP : ANIM_IDLE_RIGHT_UP;
        }
        else if (angle >= 67.5f && angle < 112.5f) // Up
        {
            return moving ? ANIM_RUN_UP : ANIM_IDLE_UP;
        }
        else if (angle >= 112.5f && angle < 157.5f) // Left-Up
        {
            return moving ? ANIM_RUN_LEFT_UP : ANIM_IDLE_LEFT_UP;
        }
        else if (angle >= 157.5f && angle < 202.5f) // Left
        {
            return moving ? ANIM_RUN_LEFT : ANIM_IDLE_LEFT;
        }
        else if (angle >= 202.5f && angle < 247.5f) // Left-Down
        {
            return moving ? ANIM_RUN_LEFT_DOWN : ANIM_IDLE_LEFT_DOWN;
        }
        else if (angle >= 247.5f && angle < 292.5f) // Down
        {
            return moving ? ANIM_RUN_DOWN : ANIM_IDLE_DOWN;
        }
        else // Right-Down (292.5f to 337.5f)
        {
            return moving ? ANIM_RUN_RIGHT_DOWN : ANIM_IDLE_RIGHT_DOWN;
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Check if animation exists
            bool animationExists = false;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    animationExists = true;
                    break;
                }
            }

            if (animationExists)
            {
                animator.Play(animationName);
                Debug.Log($"Playing animation: {animationName}");
            }
            else
            {
                Debug.LogWarning($"Animation '{animationName}' not found! Available animations:");
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    Debug.Log($"- {clip.name}");
                }
            }
        }
        else
        {
            Debug.LogError("Animator or AnimatorController is missing!");
        }
    }

    // Fallback methods for Player Input Component (if you prefer this method)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"OnMove called with input: {moveInput}");
    }

    // Public methods for external access
    public bool IsMoving => isMoving;
    public Vector2 MoveDirection => lastDirection;
    public float MoveSpeed => moveSpeed;

    // Method to change speed (useful for power-ups, etc.)
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}