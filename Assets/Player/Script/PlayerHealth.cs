using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float invulnerabilityDuration = 1f;

    private float currentHealth;
    private bool isDead = false;
    private PlayerStateManager stateManager;
    private PlayerAnimationController animController;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
        animController = GetComponent<PlayerAnimationController>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || stateManager.isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        string deathAnim = GetDeathAnimationName(stateManager.lastDirection);
        animController.PlayAnimation(deathAnim);
        
        GetComponent<Collider2D>().enabled = false;
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    private string GetDeathAnimationName(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 315f || angle < 45f) return "death_right";
        if (angle >= 45f && angle < 135f) return "death_up";
        if (angle >= 135f && angle < 225f) return "death_left";
        return "death_down";
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
    }
}