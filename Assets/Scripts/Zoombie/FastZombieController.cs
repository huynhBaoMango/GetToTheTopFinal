using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;
using FishNet.Component.Animating;

public class FastZombieController : NetworkBehaviour
{
    private enum ZombieState
    {
        Walking,
        Attacking,
        Ragdoll
    }

    [SerializeField] private float moveForce = 5f;
    [SerializeField] private float maxMoveSpeed = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private string redPillarTag = "RedPillar";
    [SerializeField] private float pillarFollowDistance = 8f;
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private float obstacleDestroyDelay = 0.5f;
    [SerializeField] private string worldObjectsTag = "WorldObjects";

    private Rigidbody[] _ragdollRigidbodies;
    private ZombieState _currentState = ZombieState.Walking;
    private Animator _animator;
    private NetworkAnimator _animator2;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigid;
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

        if (TryGetComponent(out Rigidbody rigidbody))
        {
            _rigid = rigidbody;
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
        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);

        _navMeshAgent.isStopped = false;

        if (distanceToTarget <= attackRange)
        {
            _currentState = ZombieState.Attacking;
            _lastAttackTime = Time.time;
        }

        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 30 * Time.deltaTime);

        // Kiểm tra chướng ngại vật
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance))
        {
            if (hit.collider.CompareTag(worldObjectsTag))
            {
                Debug.Log("Obstacle detected");
                StartCoroutine(DestroyObstacle(hit.collider.GetComponent<ObstacleHealth>()));
            }
        }
    }

    private IEnumerator DestroyObstacle(ObstacleHealth obstacleHealth)
    {
        // Gọi animation tấn công
        _animator.SetTrigger("Attack");
        _animator2.SetTrigger("Attack");

        yield return new WaitForSeconds(obstacleDestroyDelay);

        if (obstacleHealth != null)
        {
            obstacleHealth.TakeDamageServerRpc(attackDamage);
        }
    }

    private Transform GetRedPillar()
    {
        GameObject redPillar = GameObject.FindGameObjectWithTag(redPillarTag);
        return redPillar != null ? redPillar.transform : null;
    }

    private void AttackingBehaviour()
    {
        if (_currentTarget == null)
        {
            _navMeshAgent.isStopped = false;
            _currentState = ZombieState.Walking;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);

        // Nếu mục tiêu ngoài phạm vi, quay lại trạng thái Walking
        if (distanceToTarget > attackRange)
        {
            _currentState = ZombieState.Walking;
            _navMeshAgent.isStopped = false;
            return;
        }

        // Thực hiện tấn công nếu cooldown cho phép
        if (Time.time - _lastAttackTime >= attackCooldown)
        {
            if (_currentTarget.CompareTag("Player"))
            {
                if (_currentTarget.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
            else if (_currentTarget.CompareTag(redPillarTag))
            {
               
                Debug.Log("Zombie đang tấn công cột đỏ!");
                
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
private void RagdollBehaviour()
    {
        // Thêm hành vi cho trạng thái Ragdoll tại đây nếu cần.
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
}