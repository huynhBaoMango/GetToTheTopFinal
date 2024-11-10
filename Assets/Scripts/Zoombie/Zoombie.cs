﻿using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    private enum ZombieState
    {
        Walking,
        Attacking,
        Ragdoll
    }

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private float attackRange = 2f;
    [SerializeField]
    private float attackCooldown = 2f;

    private Rigidbody[] _ragdollRigidbodies;
    private ZombieState _currentState = ZombieState.Walking;
    private Animator _animator;
    private CharacterController _characterController;
    private NavMeshAgent _navMeshAgent;

    private float _lastAttackTime;

    void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        DisableRagdoll();
    }

    void Update()
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

        Rigidbody hitRigidbody = _ragdollRigidbodies.OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPoint)).First();

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
        _characterController.enabled = true;
        _navMeshAgent.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }

        _animator.enabled = false;
        _characterController.enabled = false;
        _navMeshAgent.enabled = false;
    }

    private void WalkingBehaviour()
    {
        if (_currentState != ZombieState.Attacking)
        {
            _navMeshAgent.SetDestination(_player.position);  // Thiết lập đích đến là player
        }

        Vector3 direction = _camera.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distanceToPlayer <= attackRange && Time.time - _lastAttackTime >= attackCooldown)
        {
            _currentState = ZombieState.Attacking;
        }
    }

    private void AttackingBehaviour()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            // Nếu zombie đang trong animation tấn công, không làm gì thêm
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
        // Cần thêm các hành vi cho ragdoll ở đây nếu cần
    }
}
