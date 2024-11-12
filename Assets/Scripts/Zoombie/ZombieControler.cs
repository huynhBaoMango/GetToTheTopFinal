using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class ZombieControler : NetworkBehaviour
{
    private enum ZombieState
    {
        Walking,
        Attacking,
        Ragdoll
    }

    [SerializeField] private float moveForce = 3f;
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    private Rigidbody[] _ragdollRigidbodies;
    private ZombieState _currentState = ZombieState.Walking;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigid;
    private float _lastAttackTime;
    private Transform _currentTarget;

    private void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        DisableRagdoll();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsServer)
        {
            enabled = false;
            return;
        }

        if (TryGetComponent(out Rigidbody rigidbody))
        {
            _rigid = rigidbody;
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

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
        Transform closestPlayer = GetClosestPlayer();
        if (closestPlayer == null)
        {
            return;
        }

        _currentTarget = closestPlayer;

        _navMeshAgent.SetDestination(_currentTarget.position);
        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);

        if (distanceToTarget <= attackRange)
        {
            _currentState = ZombieState.Attacking;
            _navMeshAgent.isStopped = true;
            _animator.SetTrigger("Attack");
            _lastAttackTime = Time.time;
        }
        else
        {
            _navMeshAgent.isStopped = false;
        }

        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);
    }

    private void AttackingBehaviour()
    {
        if (_currentTarget == null)
        {
            _currentState = ZombieState.Walking;
            _navMeshAgent.isStopped = false;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);

        _animator.SetFloat("Distance", distanceToTarget);

        if (Time.time - _lastAttackTime >= attackCooldown && distanceToTarget <= attackRange)
        {
            if (_currentTarget.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(attackDamage); // Gọi phương thức TakeDamage để giảm máu của player
            }

            _animator.SetTrigger("Attack");
            _lastAttackTime = Time.time;
        }

        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);

        if (distanceToTarget > attackRange)
        {
            _currentState = ZombieState.Walking;
            _navMeshAgent.isStopped = false;
        }
    }

    private void RagdollBehaviour()
    {
        // Thêm hành vi cho trạng thái Ragdoll tại đây nếu cần.
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
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
