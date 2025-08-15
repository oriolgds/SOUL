using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float patrolDistance = 3f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTime = 2f;

    private float currentHealth;
    private bool isDead = false;
    private bool isHurt = false;
    private Vector2 lastDirection = Vector2.down;
    
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    
    private enum State { Patrol, Chase, Attack, Hurt, Dead }
    private State currentState = State.Patrol;
    
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private Vector2 patrolStartPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        patrolStartPos = transform.position;
        
        if (patrolPoints.Length == 0)
        {
            patrolPoints = new Transform[2];
            GameObject point1 = new GameObject("PatrolPoint1");
            GameObject point2 = new GameObject("PatrolPoint2");
            point1.transform.position = patrolStartPos + Vector2.left * patrolDistance;
            point2.transform.position = patrolStartPos + Vector2.right * patrolDistance;
            patrolPoints[0] = point1.transform;
            patrolPoints[1] = point2.transform;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;
        PlayAnimation("idle_down");
    }

    private void Update()
    {
        if (isDead) return;

        FindPlayer();
        HandleState();
        HandleAnimation();
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>()?.transform;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange && currentState != State.Hurt)
        {
            if (distanceToPlayer <= attackRange)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
        else if (currentState == State.Chase && distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = State.Patrol;
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Vector2 targetPos = patrolPoints[currentPatrolIndex].position;
        float distance = Vector2.Distance(transform.position, targetPos);

        if (distance > 0.5f)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * walkSpeed;
            lastDirection = direction;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
    }

    private void Chase()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * runSpeed;
        lastDirection = direction;
    }

    private void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsInvulnerable)
            {
                Vector2 damageDirection = (player.position - transform.position).normalized;
                playerController.TakeDamage(damageDirection, 1f);
            }
        }
        
        currentState = State.Patrol;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isHurt) return;

        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtState());
        }
    }

    private System.Collections.IEnumerator HurtState()
    {
        isHurt = true;
        currentState = State.Hurt;
        rb.linearVelocity = Vector2.zero;
        
        string hurtAnim = GetHurtAnimationName(lastDirection);
        PlayAnimation(hurtAnim);
        
        yield return new WaitForSeconds(0.5f);
        
        isHurt = false;
        currentState = State.Patrol;
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        string deathAnim = GetDeathAnimationName(lastDirection);
        PlayAnimation(deathAnim);
        
        GetComponent<Collider2D>().enabled = false;
        
        Destroy(gameObject, 2f);
    }

    private void HandleAnimation()
    {
        if (isDead || isHurt) return;

        string animToPlay = "";
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        switch (currentState)
        {
            case State.Patrol:
                animToPlay = isMoving ? GetWalkAnimationName(lastDirection) : GetIdleAnimationName(lastDirection);
                break;
            case State.Chase:
                animToPlay = GetRunAnimationName(lastDirection);
                break;
            case State.Attack:
                animToPlay = GetIdleAnimationName(lastDirection);
                break;
        }

        PlayAnimation(animToPlay);
    }

    private string GetIdleAnimationName(Vector2 direction)
    {
        return "idle_" + GetDirectionSuffix(direction);
    }

    private string GetWalkAnimationName(Vector2 direction)
    {
        return "walk_" + GetDirectionSuffix(direction);
    }

    private string GetRunAnimationName(Vector2 direction)
    {
        return "run_" + GetDirectionSuffix(direction);
    }

    private string GetHurtAnimationName(Vector2 direction)
    {
        return "hurt_" + GetDirectionSuffix(direction);
    }

    private string GetDeathAnimationName(Vector2 direction)
    {
        return "death_" + GetDirectionSuffix(direction);
    }

    private string GetDirectionSuffix(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 315f || angle < 45f) return "right";
        if (angle >= 45f && angle < 135f) return "up";
        if (angle >= 135f && angle < 225f) return "left";
        return "down";
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null)
            animator.Play(animationName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}