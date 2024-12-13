using UnityEngine;
using FishNet.Object;

public class TurretRotation : NetworkBehaviour
{
    public Transform target;
    public Transform rotationPivot;
    public Transform firePoint;
    public ParticleSystem flameParticles;
    public float rotationSpeed = 5f; // Tăng tốc độ xoay
    public float detectionRange = 10f;
    public float damage = 40f;
    private float targetSwitchCooldown = 1f; // Thời gian giữ mục tiêu hiện tại
    private float timeSinceLastTargetSwitch = 0f;

    void Update()
    {
        if (!IsServer) return;

        timeSinceLastTargetSwitch += Time.deltaTime;

        if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
        {
            RotateAndFire();
        }
        else
        {
            FindClosestTarget();
        }
    }

    private void RotateAndFire()
    {
        // Xoay đầu súng mượt mà quanh trục Y
        Vector3 direction = target.position - rotationPivot.position;
        direction.y = 0; // Chỉ thay đổi trên trục Y
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 targetRotation = Quaternion.Slerp(rotationPivot.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;
        rotationPivot.rotation = Quaternion.Euler(0f, targetRotation.y, 0f);

        // Phun lửa và kiểm tra raycast
        if (!flameParticles.isPlaying)
            flameParticles.Play();

        RaycastHit hit;
        Vector3 rayDirection = firePoint.forward;
        Debug.DrawRay(firePoint.position, rayDirection * detectionRange, Color.red);

        if (Physics.Raycast(firePoint.position, rayDirection, out hit, detectionRange))
        {
            Debug.Log("Raycast trúng mục tiêu: " + hit.collider.name);

            ZombieHealth zombieHealth = hit.collider.GetComponent<ZombieHealth>();
            if (zombieHealth != null)
            {
                Debug.Log("Gây sát thương lên ZombieHealth");
                CmdDealDamage(zombieHealth, (int)damage);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy ZombieHealth trên đối tượng bị raycast trúng.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdDealDamage(ZombieHealth zombieHealth, int damage)
    {
        zombieHealth.TakeDamage(damage);
    }

    private void FindClosestTarget()
    {
        if (timeSinceLastTargetSwitch < targetSwitchCooldown && target != null)
        {
            return;
        }

        ZombieHealth[] zombies = FindObjectsOfType<ZombieHealth>();
        float closestDistance = Mathf.Infinity;
        ZombieHealth closestZombie = null;

        foreach (ZombieHealth zombie in zombies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, zombie.transform.position);

            if (distanceToEnemy < closestDistance && distanceToEnemy <= detectionRange)
            {
                closestDistance = distanceToEnemy;
                closestZombie = zombie;
            }
        }

        if (closestZombie != null)
        {
            target = closestZombie.transform;
            timeSinceLastTargetSwitch = 0f;
        }
        else
        {
            target = null;
        }
    }
}
