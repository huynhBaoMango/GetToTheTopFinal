using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class ZombieController
{
    private enum ZombieState
    {
        Walking,
        Ragdoll,
        StandingUp,
        ResettingBones,
        Attacking
    }
    private bool isAttackingObstacle = false;
    private float _obstacleDetectionRange = 0.2f; // Khoảng cách để phát hiện vật cản
    private string _obstacleTag = "Obstacle"; // Tag của vật cản
    private int _obstacleHealth = 1;

    private ZombieState _currentState = ZombieState.Walking;
    private float _timeToWakeUp;
    private float _elapsedResetBonesTime;
    private bool _isFacingUp;

    private float _timeToResetBones;
    private Transform[] _bones;
    private BoneTransform[] _faceUpStandUpBoneTransforms;
    private BoneTransform[] _faceDownStandUpBoneTransforms;
    private BoneTransform[] _ragdollBoneTransforms;
    private Transform _hipsBone;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Transform _playerTarget;
    private float _chaseSpeed;
    private string _attackTriggerName;
    private Rigidbody[] _ragdollRigidbodies;
    private CharacterController _characterController;
    private string _faceUpStandUpStateName;
    private string _faceDownStandUpStateName;

    public ZombieController(
        float timeToResetBones,
        Transform[] bones,
        BoneTransform[] faceUpStandUpBoneTransforms,
        BoneTransform[] faceDownStandUpBoneTransforms,
        BoneTransform[] ragdollBoneTransforms,
        Transform hipsBone,
        Animator animator,
        NavMeshAgent navMeshAgent,
        Transform playerTarget,
        float chaseSpeed,
        string attackTriggerName,
        Rigidbody[] ragdollRigidbodies,
        CharacterController characterController,
        string faceUpStandUpStateName,
        string faceDownStandUpStateName
    )
    {
        _timeToResetBones = timeToResetBones;
        _bones = bones;
        _faceUpStandUpBoneTransforms = faceUpStandUpBoneTransforms;
        _faceDownStandUpBoneTransforms = faceDownStandUpBoneTransforms;
        _ragdollBoneTransforms = ragdollBoneTransforms;
        _hipsBone = hipsBone;
        _animator = animator;
        _navMeshAgent = navMeshAgent;
        _playerTarget = playerTarget;
        _chaseSpeed = chaseSpeed;
        _attackTriggerName = attackTriggerName;
        _ragdollRigidbodies = ragdollRigidbodies;
        _characterController = characterController;
        _faceUpStandUpStateName = faceUpStandUpStateName;
        _faceDownStandUpStateName = faceDownStandUpStateName;
    }

    public void Update()
    {
        switch (_currentState)
        {
            case ZombieState.Walking:
                WalkingBehaviour();
                DetectAndAttackObstacles();
                break;
            case ZombieState.Ragdoll:
                RagdollBehaviour();
                break;
            case ZombieState.StandingUp:
                StandingUpBehaviour();
                break;
            case ZombieState.ResettingBones:
                ResettingBonesBehaviour();
                break;
            case ZombieState.Attacking:
                AttackingBehaviour();
                break;
        }
    }
    private void WalkingBehaviour()
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(_playerTarget.position);
        _navMeshAgent.speed = _chaseSpeed;
        Vector3 directionToPlayer = (_playerTarget.position - _navMeshAgent.transform.position).normalized;
        Quaternion toRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        _navMeshAgent.transform.rotation = Quaternion.RotateTowards(_navMeshAgent.transform.rotation, toRotation, 20 * Time.deltaTime);

        // Thay đổi khoảng cách để phát hiện vật cản
        if (Vector3.Distance(_navMeshAgent.transform.position, _playerTarget.position) < 1.5f)
        {
            _navMeshAgent.isStopped = true;
            _animator.SetTrigger(_attackTriggerName);
            _currentState = ZombieState.Attacking;
        }
        else
        {
            DetectAndAttackObstacles(); // Gọi phương thức phát hiện và tấn công vật cản khi không tấn công player
        }
    }
    private void AttackObstacle(Collider obstacle)
    {
        if (isAttackingObstacle) return; // Kiểm tra nếu đã tấn công, không gọi lại nữa

        isAttackingObstacle = true; // Đặt cờ khi bắt đầu tấn công
        _animator.SetTrigger(_attackTriggerName);
        if (obstacle.TryGetComponent(out DoorHealth obstacleHealth))
        {
            CoroutineHelper.Instance.ExecuteCoroutine(DestroyObstacleAfterDelay(0.5f, obstacleHealth, obstacle));
        }
    }

    private IEnumerator DestroyObstacleAfterDelay(float delay, DoorHealth obstacleHealth, Collider obstacle)
    {
        yield return new WaitForSeconds(delay);
        if (obstacleHealth != null)
        {
            obstacleHealth.TakeDamage();
            if (obstacleHealth.CurrentHealth > 0) // Kiểm tra nếu vật cản vẫn còn sức khỏe
            {
                // Gọi lại AttackObstacle để tiếp tục tấn công
                isAttackingObstacle = false; // Đặt lại cờ để có thể tấn công lại
                AttackObstacle(obstacle);
            }
            else
            {
                isAttackingObstacle = false; // Đặt lại cờ khi vật cản bị phá hủy
            }
        }
    }

    private void DetectAndAttackObstacles()
    {
        RaycastHit hit;
        if (Physics.SphereCast(_navMeshAgent.transform.position, 0.5f, _navMeshAgent.transform.forward, out hit, _obstacleDetectionRange))
        {
            if (hit.collider.CompareTag(_obstacleTag))
            {
                if (!isAttackingObstacle) // Chỉ gọi tấn công nếu chưa tấn công
                {
                    AttackObstacle(hit.collider);
                }
            }
        }
    }

    private void RagdollBehaviour()
    {
        _timeToWakeUp -= Time.deltaTime;

        if (_timeToWakeUp <= 0)
        {
            _isFacingUp = _hipsBone.forward.y > 0;
            AlignRotationToHips();
            PopulateBoneTransforms(_ragdollBoneTransforms);
            _currentState = ZombieState.ResettingBones;
            _elapsedResetBonesTime = 0;
        }
    }

    private void StandingUpBehaviour()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName(GetStandUpStateName()) == false)
        {
            _currentState = ZombieState.Walking;
        }
    }

    private void ResettingBonesBehaviour()
    {
        _elapsedResetBonesTime += Time.deltaTime;
        float elapsedPercentage = _elapsedResetBonesTime / _timeToResetBones;
        BoneTransform[] standUpBoneTransforms = GetStandUpBoneTransforms();

        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            _bones[boneIndex].localPosition = Vector3.Lerp(
                _ragdollBoneTransforms[boneIndex].Position,
                standUpBoneTransforms[boneIndex].Position,
                elapsedPercentage);

            _bones[boneIndex].localRotation = Quaternion.Lerp(
                _ragdollBoneTransforms[boneIndex].Rotation,
                standUpBoneTransforms[boneIndex].Rotation,
                elapsedPercentage);
        }

        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            _bones[boneIndex].localScale = Vector3.one;
        }

        if (elapsedPercentage >= 1)
        {
            _currentState = ZombieState.StandingUp;
            DisableRagdoll();
            _animator.Play(GetStandUpStateName(), 0, 0);
        }
    }

    private void AttackingBehaviour()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsTag("Attack")) // Kiểm tra nếu animation tấn công không còn phát
        {
            _currentState = ZombieState.Walking;
            _navMeshAgent.isStopped = false;
            isAttackingObstacle = false; // Đặt lại cờ sau khi hoàn thành tấn công
        }
    }


    public void TriggerRagdoll()
    {
        EnableRagdoll();
        _timeToWakeUp = UnityEngine.Random.Range(5, 10);
        _currentState = ZombieState.Ragdoll;
    }

    private void AlignRotationToHips()
    {
        Vector3 originalHipsPosition = _hipsBone.position;
        Quaternion originalHipsRotation = _hipsBone.rotation;
        Vector3 desiredDirection = _hipsBone.up;

        if (_isFacingUp)
        {
            desiredDirection *= -1;
        }

        desiredDirection.y = 0;
        desiredDirection.Normalize();
        Quaternion fromToRotation = Quaternion.FromToRotation(_navMeshAgent.transform.forward, desiredDirection);
        _navMeshAgent.transform.rotation *= fromToRotation;
        _hipsBone.position = originalHipsPosition;
        _hipsBone.rotation = originalHipsRotation;
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
    }

    private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
    {
        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            boneTransforms[boneIndex].Position = _bones[boneIndex].localPosition;
            boneTransforms[boneIndex].Rotation = _bones[boneIndex].localRotation;
        }
    }

    private string GetStandUpStateName()
    {
        return _isFacingUp ? _faceUpStandUpStateName : _faceDownStandUpStateName;
    }

    private BoneTransform[] GetStandUpBoneTransforms()
    {
        return _isFacingUp ? _faceUpStandUpBoneTransforms : _faceDownStandUpBoneTransforms;
    }
}