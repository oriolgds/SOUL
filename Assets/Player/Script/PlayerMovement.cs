using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rollDistance = 3f;

    private Rigidbody2D rb;
    private PlayerStateManager stateManager;
    private Vector2 rollDirection;
    private float rollStartTime;
    private float rollDuration;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stateManager = GetComponent<PlayerStateManager>();
    }

    public void HandleMovement(Vector2 moveInput)
    {
        if (rb == null) return;

        if (stateManager.isRolling)
        {
            HandleRollMovement();
        }
        else if (stateManager.IsInSpecialState())
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    public void StartRoll(Vector2 direction, float duration)
    {
        rollDirection = direction;
        rollDuration = duration;
        rollStartTime = Time.time;
    }

    private void HandleRollMovement()
    {
        float rollProgress = (Time.time - rollStartTime) / rollDuration;
        rollProgress = Mathf.Clamp01(rollProgress);

        float speedMultiplier = 1f - (rollProgress * rollProgress);
        Vector2 rollVelocity = rollDirection * (rollDistance / rollDuration) * speedMultiplier;

        rb.linearVelocity = rollVelocity;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public float MoveSpeed => moveSpeed;
}