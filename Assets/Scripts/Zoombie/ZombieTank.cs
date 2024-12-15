using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;
using FishNet.Component.Animating;
using System.Collections;

public class ZombieTank : NetworkBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float attackSpeed = 0f; // Tốc độ khi tấn công (0 để đứng yên)
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private string redPillarTag = "RedPillar";
    [SerializeField] private float pillarFollowDistance = 8f;
    [SerializeField] private float pillarAttackRange = 1f;
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private float obstacleDestroyDelay = 1f;
    [SerializeField] private string worldObjectsTag = "WorldObjects";

    private Rigidbody[] _ragdollRigidbodies;
    private enum ZombieState
    {
        Walking,
        Attacking,
        Ragdoll
    }
    private ZombieState _currentState = ZombieState.Walking;
    private Animator _animator;
    private NetworkAnimator _animator2;
    private NavMeshAgent _navMeshAgent;
    private float _lastAttackTime;
    private Transform _currentTarget;


    private void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _animator2 = GetComponent<NetworkAnimator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        DisableRagdoll();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServerInitialized)
        {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        switch (_currentState)
        {
            case ZombieState.Walking:
                WalkingBehaviour();
                break;
            case ZombieState.Attacking:
                AttackingBehaviour();
                break;
            case ZombieState.Ragdoll:
                RagdollBehaviour();
                break;
        }
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();
        Rigidbody hitRigidbody = _ragdollRigidbodies.OrderBy(rb => Vector3.Distance(rb.position, hitPoint)).First();
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        _currentState = ZombieState.Ragdoll;
    }

    private void DisableRagdoll()
    {
        foreach (Rigidbody rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        _animator.enabled = true;
        _navMeshAgent.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (Rigidbody rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }
        _animator.enabled = false;
        _navMeshAgent.enabled = false;
    }

    private void WalkingBehaviour()
    {
        Transform target = null;
        Transform closestPlayer = GetClosestPlayer();

        if (closestPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.position);
            target = distanceToPlayer <= pillarFollowDistance ? closestPlayer : GetRedPillar();
        }
        else
        {
            target = GetRedPillar();
        }

        if (target == null)
        {
            return;
        }

        _currentTarget = target;
        _navMeshAgent.SetDestination(_currentTarget.position);

        _navMeshAgent.speed = walkSpeed;
        _navMeshAgent.isStopped = false;

        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);



        if (distanceToTarget <= attackRange && _currentTarget == closestPlayer)
        {
            _currentState = ZombieState.Attacking;
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");
            _lastAttackTime = Time.time;
        }
        else if (distanceToTarget <= pillarAttackRange && _currentTarget == GetRedPillar())
        {
            _currentState = ZombieState.Attacking;
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");
            _lastAttackTime = Time.time;
        }

        // Xoay zombie về phía mục tiêu
        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 20 * Time.deltaTime);

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 2, transform.forward, out hit, obstacleCheckDistance))
        {
            if (hit.collider.TryGetComponent<ObstacleHealth>(out ObstacleHealth obH))
            {
                StartCoroutine(DestroyObstacle(obH));
            }
        }
    }

    private void AttackingBehaviour()
    {
        _currentTarget = GetHighestPriorityTarget();
        if (_currentTarget == null)
        {
            _navMeshAgent.isStopped = false;
            _currentState = ZombieState.Walking;
            return;
        }


        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);

        _navMeshAgent.isStopped = true;
        _navMeshAgent.speed = attackSpeed;
        _navMeshAgent.velocity = Vector3.zero;


        if ((distanceToTarget > attackRange && _currentTarget.CompareTag("Player")) ||
            (distanceToTarget > pillarAttackRange && _currentTarget.CompareTag(redPillarTag)))
        {
            _currentState = ZombieState.Walking;
            _navMeshAgent.isStopped = false;
            return;
        }


        if (Time.time - _lastAttackTime >= attackCooldown)
        {
            if (_currentTarget.CompareTag("Player") && _currentTarget.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                playerHealth.ChangeCurrentHealth(-20f);
            }
            else if (_currentTarget.CompareTag(redPillarTag) && _currentTarget.TryGetComponent<RedPillarHealth>(out RedPillarHealth pillarHealth))
            {
                pillarHealth.TakeDamage(attackDamage);
            }
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");

            _lastAttackTime = Time.time;
        }
        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 20 * Time.deltaTime);


    }



    private IEnumerator DestroyObstacle(ObstacleHealth obstacleHealth)
    {


        yield return new WaitForSeconds(obstacleDestroyDelay);

        if (obstacleHealth != null)
        {
            obstacleHealth.TakeDamageServerRpc(attackDamage);
        }
    }


    private void RagdollBehaviour()
    {
        // Add ragdoll behavior here if needed. For example, a timer to get up.
    }



    private Transform GetRedPillar()
    {
        GameObject redPillar = GameObject.FindGameObjectWithTag(redPillarTag);
        return redPillar != null ? redPillar.transform : null;
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0)
        {
            return null;
        }

        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        return closestPlayer;
    }

    private Transform GetHighestPriorityTarget()
    {
        Transform closestPlayer = GetClosestPlayer();
        float distanceToPlayer = closestPlayer != null ? Vector3.Distance(transform.position, closestPlayer.position) : Mathf.Infinity;
        Transform redPillar = GetRedPillar();
        float distanceToPillar = redPillar != null ? Vector3.Distance(transform.position, redPillar.position) : Mathf.Infinity;


        if (closestPlayer != null && distanceToPlayer <= attackRange)
        {
            return closestPlayer;
        }
        else if (redPillar != null && distanceToPillar <= pillarAttackRange)
        {
            return redPillar;
        }

        return distanceToPlayer < distanceToPillar ? closestPlayer : redPillar;


    }

}