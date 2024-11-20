using System.Linq;
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
    [SerializeField] private string redPillarTag = "RedPillar";
    [SerializeField] private float pillarFollowDistance = 6f;
    [SerializeField] private string worldObjectTag = "WorldObjects";
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private float obstacleDestroyDelay = 1f;

    private Rigidbody[] _ragdollRigidbodies;
    private ZombieState _currentState = ZombieState.Walking;
    private Animator _animator;
    private NetworkAnimator _animator2;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigid;
    private float _lastAttackTime;
    private Transform _currentTarget;
    private float _lastObstacleCheckTime;
    private GameObject _currentObstacle;
    private bool _isAttackingObstacle = false;

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

        NavMeshPath sentiero = new NavMeshPath();
        _navMeshAgent.CalculatePath(_currentTarget.position, sentiero);
        if (sentiero.status == NavMeshPathStatus.PathPartial)
        {
            CheckAndDestroyObstacle();
            _lastObstacleCheckTime = Time.time;
            return;
        }
  

        _navMeshAgent.SetDestination(_currentTarget.position);
        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        _animator.SetFloat("Distance", distanceToTarget);

        if (_currentObstacle != null && _navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            _navMeshAgent.isStopped = true;
            _isAttackingObstacle = true;
            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                AttackObstacle();
                _lastAttackTime = Time.time;
            }
            return;
        }

        _isAttackingObstacle = false;
        _navMeshAgent.isStopped = false;


        if (distanceToTarget <= attackRange && _currentTarget == closestPlayer)
        {
            _currentState = ZombieState.Attacking;
            _animator.SetTrigger("Attack");
            _animator2.SetTrigger("Attack");
            _lastAttackTime = Time.time;
        }

        Vector3 direction = _currentTarget.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 20 * Time.deltaTime);
    }

    private Transform GetRedPillar()
    {
        GameObject redPillar = GameObject.FindGameObjectWithTag(redPillarTag);
        return redPillar != null ? redPillar.transform : null;
    }


    private void AttackObstacle()
    {

        if (_currentObstacle != null)
        {


            _currentObstacle.GetComponent<ObstacleHealth>().TakeDamageRpc(1);

        }


    }


    private void CheckAndDestroyObstacle()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag(worldObjectTag);
        float lastDistance = 100f;
        Transform taretObj = null;
        foreach (GameObject obj2 in obj)
        {
            if(Vector3.Distance(transform.position, obj2.transform.position) < lastDistance)
            {
                lastDistance = Vector3.Distance(transform.position, obj2.transform.position);
                taretObj = obj2.transform;
            }
        }
        if(taretObj != null) _navMeshAgent.SetDestination(taretObj.position);
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




        if (distanceToTarget > attackRange)
        {
            _currentState = ZombieState.Walking;
            _navMeshAgent.isStopped = false;

            return;

        }




        if (Time.time - _lastAttackTime >= attackCooldown)
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