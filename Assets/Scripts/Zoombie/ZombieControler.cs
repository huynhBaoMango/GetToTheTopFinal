﻿using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;
using FishNet.Component.Animating;

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
    [SerializeField] private float attackRange;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private string redPillarTag = "RedPillar"; // Tag của cột đỏ
    [SerializeField] private float pillarFollowDistance = 6f;

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

            if (distanceToPlayer <= pillarFollowDistance)
            {
                target = closestPlayer;
            }
            else
            {
                GameObject redPillar = GameObject.FindGameObjectWithTag(redPillarTag);
                if (redPillar != null)
                {
                    target = redPillar.transform;
                }
            }
        }
        else
        {
            GameObject redPillar = GameObject.FindGameObjectWithTag(redPillarTag);
            if (redPillar != null)
            {
                target = redPillar.transform;
            }
        }

        if (target == null)
        {
            return;
        }

        _currentTarget = target;

        _navMeshAgent.SetDestination(_currentTarget.position);
        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);

        if (distanceToTarget <= attackRange && _currentTarget == closestPlayer)
        {
            _navMeshAgent.isStopped = false; // Ensure agent isn't stopped before attacking
            _currentState = ZombieState.Attacking;
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack"); // Trigger NetworkAnimator as well
            _lastAttackTime = Time.time;
        }

        // Rotate towards target
        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);
    }



    private void AttackingBehaviour()
    {
        AttackingObserver();
    }

    private void AttackingObserver()
    {
        if (_currentTarget == null)
        {
            _navMeshAgent.isStopped = false;
            _currentState = ZombieState.Walking;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);

        _animator.SetFloat("Distance", distanceToTarget);

        if (Time.time - _lastAttackTime >= attackCooldown && distanceToTarget <= attackRange)
        {
            if (_currentTarget.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
            }

            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");
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