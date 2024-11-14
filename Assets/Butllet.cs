using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    private List<PreformantBullet> _spawnedBullets = new List<PreformantBullet>();

    private void Update()
    {
        for (int i = _spawnedBullets.Count - 1; i >= 0; i--)
        {
            PreformantBullet bullet = _spawnedBullets[i];
            bullet.BulletTransform.position += bullet.Direction * Time.deltaTime * bulletSpeed;

            
            if (Physics.Raycast(bullet.BulletTransform.position, bullet.Direction, out RaycastHit hit, bulletSpeed * Time.deltaTime))
            {
                if (hit.transform.TryGetComponent(out ZombieHealth zombieHealth))
                {
                    Debug.Log($"Bullet hit zombie at position: {hit.point}");
                    zombieHealth.TakeDamage(bullet.Damage);
                    Destroy(bullet.BulletTransform.gameObject);
                    _spawnedBullets.RemoveAt(i); 
                }
            }
        }
    }

    public void Shoot(Vector3 startPosition, Vector3 direction, int damage)
    {
        SpawnBullet(startPosition, direction, TimeManager.Tick, damage);

    }

    public void SpawnBulletLocal(Vector3 startPosition, Vector3 direction, int damage)
    {
        GameObject bullet = Instantiate(bulletPrefab, startPosition, Quaternion.identity);
        _spawnedBullets.Add(new PreformantBullet() { BulletTransform = bullet.transform, Direction = direction, Damage = damage });
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBullet(Vector3 startPosition, Vector3 direction, uint startTick, int damage)
    {
        SpawnBulletObserver(startPosition, direction, startTick, damage);
    }

    private void SpawnBulletObserver(Vector3 startPosition, Vector3 direction, uint startTick, int damage)
    {
        float timeDifference = (float)(TimeManager.Tick - startTick) / TimeManager.TickRate;
        Vector3 spawnPosition = startPosition + direction * bulletSpeed * timeDifference;
        NetworkObject newBullet = NetworkManager.GetPooledInstantiated(bulletPrefab, true);
        ServerManager.Spawn(newBullet.gameObject);
        _spawnedBullets.Add(new PreformantBullet() { BulletTransform = newBullet.gameObject.transform, Direction = direction, Damage = damage });
    }

    public class PreformantBullet
    {
        public Transform BulletTransform;
        public Vector3 Direction;
        public int Damage;
    }
}
