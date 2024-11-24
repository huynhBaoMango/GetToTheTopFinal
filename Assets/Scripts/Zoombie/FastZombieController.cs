using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using FishNet.Component.Animating;

public class FastZombieController : NetworkBehaviour
{
    [SerializeField] private float moveForce = 3f;
    [SerializeField] private float maxMoveSpeed = 10f;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private string redPillarTag = "RedPillar";
    [SerializeField] private float pillarFollowDistance = 6f;
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private float obstacleDestroyDelay = 1f;
    [SerializeField] private float obstacleAttackDamage = 50f;
    [SerializeField] private string worldObjectTag = "WorldObjects";

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
    private Rigidbody _rigid;
    private float _lastAttackTime;
    private Transform _currentTarget;
    private bool _isDestroyingObstacle = false;


    private void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _animator2 = GetComponent<NetworkAnimator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = maxMoveSpeed;
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
                // Implement ragdoll behavior if needed
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
            rigidbody.isKinematic = true;

        _animator.enabled = true;
        _navMeshAgent.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (Rigidbody rigidbody in _ragdollRigidbodies)
            rigidbody.isKinematic = false;

        _animator.enabled = false;
        _navMeshAgent.enabled = false;
    }

    private void WalkingBehaviour()
    {
        Transform target = GetClosestTarget();
        if (target == null) return;

        _currentTarget = target;

        // Prioritize attacking WorldObjects if within range
        if (_currentTarget.CompareTag(worldObjectTag) && Vector3.Distance(transform.position, _currentTarget.position) <= attackRange)
        {
            _currentState = ZombieState.Attacking;
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");
            _lastAttackTime = Time.time;
            return;
        }

        if (_navMeshAgent.destination != _currentTarget.position)
            _navMeshAgent.CalculatePath(_currentTarget.position, new NavMeshPath());

        if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            StartCoroutine(HandleObstacle());
            return;
        }

        _navMeshAgent.SetDestination(_currentTarget.position);
        _animator.SetFloat("Distance", Vector3.Distance(transform.position, _currentTarget.position));
        _navMeshAgent.isStopped = false;

        if (Vector3.Distance(transform.position, _currentTarget.position) <= attackRange)
        {
            _currentState = ZombieState.Attacking;
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");
            _lastAttackTime = Time.time;
        }

        if (_currentTarget != null) RotateTowards(_currentTarget.position);

    }

    private Transform GetClosestTarget()
    {
        Transform closestWorldObject = GetClosestWorldObject();
        if (closestWorldObject != null) return closestWorldObject;


        Transform closestPlayer = GetClosestPlayer();
        if (closestPlayer != null && Vector3.Distance(transform.position, closestPlayer.position) <= pillarFollowDistance)
            return closestPlayer;

        return GetRedPillar();
    }


    private Transform GetClosestWorldObject()
    {
        GameObject[] worldObjects = GameObject.FindGameObjectsWithTag(worldObjectTag);
        if (worldObjects.Length == 0) return null;

        Transform closestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in worldObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj.transform;
            }
        }
        return closestObject;
    }



    private Transform GetRedPillar()
    {
        GameObject redPillar = GameObject.FindGameObjectWithTag(redPillarTag);
        return redPillar != null ? redPillar.transform : null;
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return null;

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

    private void AttackingBehaviour()
    {
        Debug.Log($"Mục tiêu hiện tại: {_currentTarget?.name ?? "NULL"}");

        if (_currentTarget == null || Vector3.Distance(transform.position, _currentTarget.position) > attackRange)
        {
            _currentState = ZombieState.Walking;
            return;
        }

        if (Time.time - _lastAttackTime >= attackCooldown)
        {
            if (_currentTarget != null && _currentTarget.CompareTag(worldObjectTag))
            {
                if (_currentTarget.TryGetComponent<ObstacleHealth>(out var obstacleHealth))
                {
                    obstacleHealth.TakeDamage(Mathf.RoundToInt(obstacleAttackDamage));

                    if (obstacleHealth.IsDead())
                    {
                        _currentTarget = null;
                    }

                    _animator.SetTrigger("Attack");
                    _animator2.SetTrigger("Attack");
                    _lastAttackTime = Time.time;
                }
                else
                {
                    Debug.LogWarning($"ObstacleHealth component not found on {_currentTarget.name}");
                    _currentTarget = null;
                    _currentState = ZombieState.Walking;
                }
            }

        }

        if (_currentTarget != null) RotateTowards(_currentTarget.position);
    }


    private IEnumerator HandleObstacle()
    {
        if (_isDestroyingObstacle) yield break;
        _isDestroyingObstacle = true;
        _navMeshAgent.isStopped = true;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance))
        {
            if (hit.collider.CompareTag(worldObjectTag) && hit.collider.gameObject.TryGetComponent<NavMeshObstacle>(out var obstacle))
            {
                yield return StartCoroutine(AttackAndDestroyObstacle(obstacle.transform));
            }
        }
        else
        {
            Transform nearestWorldObject = FindNearestWorldObject();
            if (nearestWorldObject != null)
            {
                _currentTarget = nearestWorldObject;
                _navMeshAgent.SetDestination(_currentTarget.position);
                _navMeshAgent.isStopped = false;


                yield return new WaitUntil(() => _navMeshAgent.remainingDistance <= attackRange || _currentTarget == null);

                if (_currentTarget != null && _navMeshAgent.remainingDistance <= attackRange)
                {
                    yield return StartCoroutine(AttackAndDestroyObstacle(_currentTarget));

                }
            }

        }

        _isDestroyingObstacle = false;
        _navMeshAgent.isStopped = false;
        _currentState = ZombieState.Walking;

    }


    private IEnumerator AttackAndDestroyObstacle(Transform obstacle)
    {
        _currentTarget = obstacle;
        _animator.SetTrigger("Attack");
        _animator2.SetTrigger("Attack");


        yield return new WaitForSeconds(obstacleDestroyDelay);
        ApplyDamageToWorldObject();


    }

    private void ApplyDamageToWorldObject()
    {
        if (_currentTarget != null && _currentTarget.TryGetComponent<ObstacleHealth>(out var obstacleHealth))
        {
            obstacleHealth.TakeDamage(Mathf.RoundToInt(obstacleAttackDamage));
            if (obstacleHealth.IsDead())
            {
                _currentTarget = null;
            }
        }

    }


    private Transform FindNearestWorldObject()
    {
        GameObject[] worldObjects = GameObject.FindGameObjectsWithTag(worldObjectTag);
        if (worldObjects.Length == 0) return null;

        Transform nearestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in worldObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestObject = obj.transform;
            }
        }

        return nearestObject;
    }


    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 20 * Time.deltaTime);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(worldObjectTag))
        {

            StartCoroutine(AttackAndDestroyObstacle(collision.transform));

        }
    }



}
