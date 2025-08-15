using UnityEngine;
using System.Collections;

public class PlayerStateManager : MonoBehaviour
{
    [Header("State Settings")]
    [SerializeField] private float invulnerabilityDuration = 0.1f;
    [SerializeField] private bool enableDebugLogs = true;

    // State variables
    public bool isAttacking { get; private set; } = false;
    public bool isRolling { get; private set; } = false;
    public bool isShielding { get; private set; } = false;
    public bool isTakingDamage { get; private set; } = false;
    public bool isInvulnerable { get; private set; } = false;
    public bool isMoving { get; private set; } = false;

    // Attack state
    public bool useSecondAttack { get; private set; } = false;
    public bool attackInputQueued { get; private set; } = false;

    // Shield state
    public bool shieldStartPlayed { get; private set; } = false;

    // Direction tracking
    public Vector2 lastDirection { get; private set; } = Vector2.down;

    // Components
    private PlayerAnimationController animController;
    private Coroutine currentStateCoroutine;

    private void Awake()
    {
        animController = GetComponent<PlayerAnimationController>();
    }

    public void SetMoving(bool moving, Vector2 direction)
    {
        isMoving = moving;
        if (moving && !IsInSpecialState())
        {
            lastDirection = direction.normalized;
        }
    }

    public bool IsInSpecialState()
    {
        return isAttacking || isRolling || isTakingDamage || isShielding;
    }

    public void StartAttack()
    {
        if (IsInSpecialState()) return;

        string attackAnim = animController.GetAttackAnimationName(lastDirection, useSecondAttack);
        animController.PlayAnimation(attackAnim);
        
        isAttacking = true;
        useSecondAttack = !useSecondAttack;
        attackInputQueued = false;

        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);
        
        currentStateCoroutine = StartCoroutine(WaitForAttackComplete(() => {
            isAttacking = false;
            if (attackInputQueued)
            {
                attackInputQueued = false;
                StartAttack();
            }
        }));

        if (enableDebugLogs)
            Debug.Log($"Started attack: {attackAnim}");
    }

    public void StartRoll(Vector2 rollDirection)
    {
        if (IsInSpecialState()) return;

        string rollAnim = animController.GetRollingAnimationName(rollDirection);
        animController.PlayAnimation(rollAnim);
        
        isRolling = true;
        isInvulnerable = true;

        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);
        
        currentStateCoroutine = StartCoroutine(WaitForAnimationComplete(() => {
            isRolling = false;
            if (invulnerabilityDuration > 0)
            {
                StartCoroutine(InvulnerabilityCoroutine());
            }
        }));

        if (enableDebugLogs)
            Debug.Log($"Started roll: {rollAnim}");
    }

    public void StartShield()
    {
        if (isAttacking || isRolling || isTakingDamage) return;

        isShielding = true;
        shieldStartPlayed = false;

        string shieldStartAnim = animController.GetShieldAnimationName(lastDirection, true);
        animController.PlayAnimation(shieldStartAnim);

        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);
        
        currentStateCoroutine = StartCoroutine(WaitForAnimationComplete(() => {
            shieldStartPlayed = true;
            string shieldHoldAnim = animController.GetShieldAnimationName(lastDirection, false);
            animController.PlayAnimation(shieldHoldAnim);
        }));

        if (enableDebugLogs)
            Debug.Log("Shield raised");
    }

    public void StopShield()
    {
        isShielding = false;
        shieldStartPlayed = false;
        
        if (currentStateCoroutine != null)
        {
            StopCoroutine(currentStateCoroutine);
            currentStateCoroutine = null;
        }

        if (enableDebugLogs)
            Debug.Log("Shield lowered");
    }

    public void TakeDamage(Vector2 damageDirection)
    {
        if (isInvulnerable || isRolling) return;

        if (isShielding)
        {
            Vector2 shieldFacing = lastDirection;
            float dot = Vector2.Dot(damageDirection.normalized, shieldFacing.normalized);
            if (dot > 0.5f)
            {
                if (enableDebugLogs)
                    Debug.Log("Damage blocked by shield");
                return;
            }
        }

        // Stop other actions
        isAttacking = false;
        isShielding = false;
        shieldStartPlayed = false;

        string takeDamageAnim = animController.GetTakeDamageAnimationName(lastDirection);
        animController.PlayAnimation(takeDamageAnim);
        
        isTakingDamage = true;

        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);
        
        currentStateCoroutine = StartCoroutine(WaitForAnimationComplete(() => {
            isTakingDamage = false;
        }));

        StartCoroutine(PostDamageInvulnerabilityCoroutine());

        if (enableDebugLogs)
            Debug.Log($"Taking damage from direction: {damageDirection}");
    }

    public void QueueAttack()
    {
        if (isAttacking)
        {
            attackInputQueued = true;
            if (enableDebugLogs)
                Debug.Log("Attack queued for combo");
        }
        else
        {
            StartAttack();
        }
    }

    private IEnumerator WaitForAnimationComplete(System.Action onComplete)
    {
        yield return null;
        
        while (!animController.IsAnimationComplete())
        {
            yield return null;
        }
        
        onComplete?.Invoke();
        currentStateCoroutine = null;
    }

    private IEnumerator WaitForAttackComplete(System.Action onComplete)
    {
        yield return null;
        
        float normalizedTime = 0f;
        while (normalizedTime < 0.8f) // End at 80% for fluid combos
        {
            normalizedTime = animController.GetCurrentAnimationNormalizedTime();
            yield return null;
        }
        
        onComplete?.Invoke();
        currentStateCoroutine = null;
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
        
        if (enableDebugLogs)
            Debug.Log("Invulnerability ended");
    }

    private IEnumerator PostDamageInvulnerabilityCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        isInvulnerable = true;
        yield return new WaitForSeconds(0.5f);
        isInvulnerable = false;
    }

    public bool CanTakeDamageFromDirection(Vector2 damageDirection)
    {
        if (isInvulnerable || isRolling) return false;

        if (isShielding)
        {
            Vector2 shieldFacing = lastDirection;
            float dot = Vector2.Dot(damageDirection.normalized, shieldFacing.normalized);
            return dot <= 0.5f;
        }

        return true;
    }
}