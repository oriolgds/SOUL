using UnityEngine;
using System.Collections;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool enableDebugLogs = true;

    private string currentAnimation = "";

    // Animation constants
    public const string ANIM_IDLE_DOWN = "idle_down";
    public const string ANIM_IDLE_UP = "idle_up";
    public const string ANIM_IDLE_LEFT = "idle_left";
    public const string ANIM_IDLE_RIGHT = "idle_right";
    public const string ANIM_IDLE_LEFT_DOWN = "idle_left_down";
    public const string ANIM_IDLE_RIGHT_DOWN = "idle_right_down";
    public const string ANIM_IDLE_LEFT_UP = "idle_left_up";
    public const string ANIM_IDLE_RIGHT_UP = "idle_right_up";

    public const string ANIM_RUN_DOWN = "run_down";
    public const string ANIM_RUN_UP = "run_up";
    public const string ANIM_RUN_LEFT = "run_left";
    public const string ANIM_RUN_RIGHT = "run_right";
    public const string ANIM_RUN_LEFT_DOWN = "run_left_down";
    public const string ANIM_RUN_RIGHT_DOWN = "run_right_down";
    public const string ANIM_RUN_LEFT_UP = "run_left_up";
    public const string ANIM_RUN_RIGHT_UP = "run_right_up";

    public const string ANIM_ROLLING_DOWN = "rolling_down";
    public const string ANIM_ROLLING_UP = "rolling_up";
    public const string ANIM_ROLLING_LEFT = "rolling_left";
    public const string ANIM_ROLLING_RIGHT = "rolling_right";
    public const string ANIM_ROLLING_LEFT_DOWN = "rolling_left_down";
    public const string ANIM_ROLLING_RIGHT_DOWN = "rolling_right_down";
    public const string ANIM_ROLLING_LEFT_UP = "rolling_left_up";
    public const string ANIM_ROLLING_RIGHT_UP = "rolling_right_up";

    public const string ANIM_TAKEDAMAGE_DOWN = "takedamage_down";
    public const string ANIM_TAKEDAMAGE_UP = "takedamage_up";
    public const string ANIM_TAKEDAMAGE_LEFT = "takedamage_left";
    public const string ANIM_TAKEDAMAGE_RIGHT = "takedamage_right";
    public const string ANIM_TAKEDAMAGE_LEFT_DOWN = "takedamage_left_down";
    public const string ANIM_TAKEDAMAGE_RIGHT_DOWN = "takedamage_right_down";
    public const string ANIM_TAKEDAMAGE_LEFT_UP = "takedamage_left_up";
    public const string ANIM_TAKEDAMAGE_RIGHT_UP = "takedamage_right_up";

    public const string ANIM_SHIELDSTART_DOWN = "shieldstart_down";
    public const string ANIM_SHIELDSTART_UP = "shieldstart_up";
    public const string ANIM_SHIELDSTART_LEFT = "shieldstart_left";
    public const string ANIM_SHIELDSTART_RIGHT = "shieldstart_right";
    public const string ANIM_SHIELDSTART_LEFT_DOWN = "shieldstart_left_down";
    public const string ANIM_SHIELDSTART_RIGHT_DOWN = "shieldstart_right_down";
    public const string ANIM_SHIELDSTART_LEFT_UP = "shieldstart_left_up";
    public const string ANIM_SHIELDSTART_RIGHT_UP = "shieldstart_right_up";

    public const string ANIM_SHIELD_DOWN = "shield_down";
    public const string ANIM_SHIELD_UP = "shield_up";
    public const string ANIM_SHIELD_LEFT = "shield_left";
    public const string ANIM_SHIELD_RIGHT = "shield_right";
    public const string ANIM_SHIELD_LEFT_DOWN = "shield_left_down";
    public const string ANIM_SHIELD_RIGHT_DOWN = "shield_right_down";
    public const string ANIM_SHIELD_LEFT_UP = "shield_left_up";
    public const string ANIM_SHIELD_RIGHT_UP = "shield_right_up";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
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
                currentAnimation = animationName;
                if (enableDebugLogs)
                    Debug.Log($"Playing animation: {animationName}");
            }
            else
            {
                Debug.LogWarning($"Animation '{animationName}' not found!");
            }
        }
    }

    public float GetCurrentAnimationDuration()
    {
        if (animator == null) return 0f;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    public float GetCurrentAnimationNormalizedTime()
    {
        if (animator == null) return 1f;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime;
    }

    public bool IsAnimationComplete()
    {
        return GetCurrentAnimationNormalizedTime() >= 1f;
    }

    public void GetAnimationDurationAsync(string animationName, System.Action<float> callback)
    {
        StartCoroutine(GetAnimationDurationCoroutine(animationName, callback));
    }

    private IEnumerator GetAnimationDurationCoroutine(string animationName, System.Action<float> callback)
    {
        yield return null; // Wait one frame for animation to start
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(animationName))
        {
            callback(stateInfo.length);
        }
        else
        {
            callback(0.5f); // Fallback
        }
    }

    public string GetAnimationName(Vector2 direction, bool moving)
    {
        if (direction == Vector2.zero) direction = Vector2.down;
        direction = direction.normalized;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f)
            return moving ? ANIM_RUN_RIGHT : ANIM_IDLE_RIGHT;
        else if (angle >= 22.5f && angle < 67.5f)
            return moving ? ANIM_RUN_RIGHT_UP : ANIM_IDLE_RIGHT_UP;
        else if (angle >= 67.5f && angle < 112.5f)
            return moving ? ANIM_RUN_UP : ANIM_IDLE_UP;
        else if (angle >= 112.5f && angle < 157.5f)
            return moving ? ANIM_RUN_LEFT_UP : ANIM_IDLE_LEFT_UP;
        else if (angle >= 157.5f && angle < 202.5f)
            return moving ? ANIM_RUN_LEFT : ANIM_IDLE_LEFT;
        else if (angle >= 202.5f && angle < 247.5f)
            return moving ? ANIM_RUN_LEFT_DOWN : ANIM_IDLE_LEFT_DOWN;
        else if (angle >= 247.5f && angle < 292.5f)
            return moving ? ANIM_RUN_DOWN : ANIM_IDLE_DOWN;
        else
            return moving ? ANIM_RUN_RIGHT_DOWN : ANIM_IDLE_RIGHT_DOWN;
    }

    public string GetAttackAnimationName(Vector2 direction, bool useSecond)
    {
        string prefix = useSecond ? "melee2" : "melee";
        string suffix = GetDirectionSuffix(direction);
        return $"{prefix}_{suffix}";
    }

    public string GetRollingAnimationName(Vector2 direction)
    {
        string suffix = GetDirectionSuffix(direction);
        return $"rolling_{suffix}";
    }

    public string GetTakeDamageAnimationName(Vector2 direction)
    {
        string suffix = GetDirectionSuffix(direction);
        return $"takedamage_{suffix}";
    }

    public string GetShieldAnimationName(Vector2 direction, bool isStart)
    {
        string prefix = isStart ? "shieldstart" : "shield";
        string suffix = GetDirectionSuffix(direction);
        return $"{prefix}_{suffix}";
    }

    private string GetDirectionSuffix(Vector2 direction)
    {
        direction = direction == Vector2.zero ? Vector2.down : direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) return "right";
        if (angle >= 22.5f && angle < 67.5f) return "right_up";
        if (angle >= 67.5f && angle < 112.5f) return "up";
        if (angle >= 112.5f && angle < 157.5f) return "left_up";
        if (angle >= 157.5f && angle < 202.5f) return "left";
        if (angle >= 202.5f && angle < 247.5f) return "left_down";
        if (angle >= 247.5f && angle < 292.5f) return "down";
        return "right_down";
    }

    public string CurrentAnimation => currentAnimation;
}