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

    [SerializeField]
    private float attackRange = 2f;
    [SerializeField]
    private float attackCooldown = 2f;
    [SerializeField]
    private float moveForce = 3, maxMoveSpeed = 5;

    private Rigidbody[] _ragdollRigidbodies;
    private ZombieState _currentState = ZombieState.Walking;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigid;
    private float _lastAttackTime;

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

        if (!IsServer) // Chỉ để server xử lý AI
        {
            enabled = false;
            return;
        }

        if (TryGetComponent(out Rigidbody rigidbody))
            _rigid = rigidbody;
    }

    private void Update()
    {
        if (!IsServer) // Đảm bảo chỉ server điều khiển zombie
            return;

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
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }

        _animator.enabled = true;
        _navMeshAgent.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }

        _animator.enabled = false;
        _navMeshAgent.enabled = false;
    }

    private void WalkingBehaviour()
    {
        Transform closestPlayer = GetClosestPlayer();
        if (closestPlayer == null) return;

        if (_currentState != ZombieState.Attacking && _navMeshAgent.enabled)
        {
            _navMeshAgent.SetDestination(closestPlayer.position);
        }

        // Loại bỏ đoạn sử dụng _camera
        Vector3 direction = closestPlayer.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);

        float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.position);
        if (distanceToPlayer <= attackRange && Time.time - _lastAttackTime >= attackCooldown)
        {
            _currentState = ZombieState.Attacking;
        }
    }

    private void AttackingBehaviour()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            return;
        }

        _animator.SetTrigger("Attack");
        _lastAttackTime = Time.time;

        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            _currentState = ZombieState.Walking;
        }
    }

    private void RagdollBehaviour()
    {
        // Cần thêm các hành vi cho Ragdoll ở đây nếu cần
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var player in players)
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
