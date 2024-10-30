using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private string _faceUpStandUpStateName;

    [SerializeField]
    private string _faceDownStandUpStateName;

    [SerializeField]
    private string _faceUpStandUpClipName;

    [SerializeField]
    private string _faceDownStandUpClipName;

    [SerializeField]
    private string _attackTriggerName = "Attack";

    [SerializeField]
    private float _timeToResetBones;

    [SerializeField]
    private float _chaseSpeed = 3f;

    private Rigidbody[] _ragdollRigidbodies;
    private Animator _animator;
    private CharacterController _characterController;
    private Transform _hipsBone;
    private NavMeshAgent _navMeshAgent;
    private Transform _playerTarget;
    private Transform[] _bones;
    private BoneTransform[] _faceUpStandUpBoneTransforms;
    private BoneTransform[] _faceDownStandUpBoneTransforms;
    private BoneTransform[] _ragdollBoneTransforms;

    private ZombieController _zombieController;

    void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _hipsBone = _animator.GetBoneTransform(HumanBodyBones.Hips);
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        _bones = _hipsBone.GetComponentsInChildren<Transform>();

        _faceUpStandUpBoneTransforms = new BoneTransform[_bones.Length];
        _faceDownStandUpBoneTransforms = new BoneTransform[_bones.Length];
        _ragdollBoneTransforms = new BoneTransform[_bones.Length];

        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            _faceUpStandUpBoneTransforms[boneIndex] = new BoneTransform();
            _faceDownStandUpBoneTransforms[boneIndex] = new BoneTransform();
            _ragdollBoneTransforms[boneIndex] = new BoneTransform();
        }

        PopulateAnimationStartBoneTransforms(_faceUpStandUpClipName, _faceUpStandUpBoneTransforms);
        PopulateAnimationStartBoneTransforms(_faceDownStandUpClipName, _faceDownStandUpBoneTransforms);
        DisableRagdoll();

        // Initialize the ZombieController with the necessary components
        _zombieController = new ZombieController(
            _timeToResetBones,
            _bones,
            _faceUpStandUpBoneTransforms,
            _faceDownStandUpBoneTransforms,
            _ragdollBoneTransforms,
            _hipsBone,
            _animator,
            _navMeshAgent,
            _playerTarget,
            _chaseSpeed,
            _attackTriggerName,
            _ragdollRigidbodies,
            _characterController,
            _faceUpStandUpStateName,
            _faceDownStandUpStateName
        );

    }

    void Update()
    {
        _zombieController.Update();
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();
        Rigidbody hitRigidbody = FindHitRigidbody(hitPoint);
        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        _navMeshAgent.enabled = false; // Disable NavMeshAgent when falling
        _zombieController.TriggerRagdoll();
    }

    private void DestroyObstacle(Collider obstacle)
    {
        Destroy(obstacle.gameObject);
    }

    private Rigidbody FindHitRigidbody(Vector3 hitPoint)
    {
        Rigidbody closestRigidbody = null;
        float closestDistance = 0;

        foreach (var rigidbody in _ragdollRigidbodies)
        {
            float distance = Vector3.Distance(rigidbody.position, hitPoint);
            if (closestRigidbody == null || distance < closestDistance)
            {
                closestDistance = distance;
                closestRigidbody = rigidbody;
            }
        }

        return closestRigidbody;
    }

    private void DisableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        _animator.enabled = true;
        _characterController.enabled = true;
        _navMeshAgent.enabled = true; // Re-enable NavMeshAgent when standing up
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

    private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
    {
        Vector3 positionBeforeSampling = transform.position;
        Quaternion rotationBeforeSampling = transform.rotation;

        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                clip.SampleAnimation(gameObject, 0);
                PopulateBoneTransforms(boneTransforms);
                break;
            }
        }

        transform.position = positionBeforeSampling;
        transform.rotation = rotationBeforeSampling;
    }
}
