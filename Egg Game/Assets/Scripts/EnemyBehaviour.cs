using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    private int wpIndex = 0;

    [Header("Tuning")]
    [SerializeField] private float holdTime = 1.0f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 6.0f;
    [SerializeField] private float forgetTime = 2.5f;

    [Header("Detection")]
    [Tooltip("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float nearRadius = 1.0f;
    [SerializeField] private float viewDistance = 7f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float detectionWidth = 0.15f;
    [SerializeField] private LayerMask occlusionLayers;
    [SerializeField] private float enemyEyeHeight = 1.2f;
    [SerializeField] private float playerTargetHeight = 0.4f;

    [Header("Animation")]
    [SerializeField] private Transform modelWithAnimator;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.0f;
    private Animator anim;
    private float attackTimer = 0f;

    private NavMeshAgent agent;

    //Esperar
    private bool isHolding = false;
    private float holdTimer = 0f;

    //Perseguir
    private bool hasLineOfSight = false;
    private float timeSinceLastSeen = Mathf.Infinity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        anim = modelWithAnimator != null
            ? modelWithAnimator.GetComponent<Animator>()
            : GetComponentInChildren<Animator>();

        agent.speed = patrolSpeed;
    }

    void Start()
    {
        if (waypoints.Count == 0) return;
        agent.SetDestination(waypoints[wpIndex].position);
    }

    void Update()
    {
        // --- Detección ---
        if (player != null)
        {
            hasLineOfSight = CanSeePlayerCone();
            if (hasLineOfSight) timeSinceLastSeen = 0f;
            else timeSinceLastSeen += Time.deltaTime;
        }

        bool shouldChase = player != null && (hasLineOfSight || timeSinceLastSeen <= forgetTime);

        // --- Ataque ---
        attackTimer -= Time.deltaTime;

        if (shouldChase && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange && attackTimer <= 0f)
            {
                attackTimer = attackCooldown;
                if (anim != null) anim.SetTrigger("Attack");
            }
        }

        // --- PATRULLAJE ---
        if (shouldChase)
        {
            isHolding = false;
            agent.isStopped = false;

            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.speed = patrolSpeed;

            if (waypoints.Count == 0) return;

            if (isHolding)
            {
                holdTimer -= Time.deltaTime;
                if (holdTimer <= 0f)
                {
                    isHolding = false;
                    agent.isStopped = false;

                    wpIndex = (wpIndex + 1) % waypoints.Count;
                    agent.SetDestination(waypoints[wpIndex].position);
                }
            }
            else
            {
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    isHolding = true;
                    holdTimer = holdTime;

                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }
        }

        if (anim != null)
        {
            float horizontalSpeed = new Vector3(agent.velocity.x, 0f, agent.velocity.z).magnitude;
            anim.SetFloat("Speed", horizontalSpeed);
            anim.SetBool("IsHolding", isHolding);
        }
    }

    private void ResumeAfterAttack()
    {
        agent.isStopped = false;
    }

    private bool CanSeePlayerCone()
    {
        Vector3 origin = transform.position + Vector3.up * enemyEyeHeight;
        Vector3 target = player.position + Vector3.up * playerTargetHeight;

        Vector3 toTarget = target - origin;
        float dist = toTarget.magnitude;
        if (dist > viewDistance) return false;
        if (dist <= 0.01f) return true;

        Vector3 dir = toTarget / dist;

        if (dist <= nearRadius)
        {
            if (detectionWidth > 0f)
            {
                if (Physics.SphereCast(origin, detectionWidth, dir, out _, dist, occlusionLayers, QueryTriggerInteraction.Ignore))
                    return false;
            }
            else
            {
                if (Physics.Raycast(origin, dir, dist, occlusionLayers, QueryTriggerInteraction.Ignore))
                    return false;
            }
            return true;
        }

        if (dist > viewDistance) return false;

        float half = viewAngle * 0.5f;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > half) return false;

        if (detectionWidth > 0f)
        {
            if (Physics.SphereCast(origin, detectionWidth, dir, out _, dist, occlusionLayers, QueryTriggerInteraction.Ignore))
                return false;
        }
        else
        {
            if (Physics.Raycast(origin, dir, dist, occlusionLayers, QueryTriggerInteraction.Ignore))
                return false;
        }

        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * enemyEyeHeight;
        float half = viewAngle * 0.5f;

        Vector3 leftDir = Quaternion.Euler(0f, -half, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, half, 0f) * transform.forward;

        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
        Gizmos.DrawWireSphere(origin, 0.05f);
    }
#endif
}