using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

namespace Roundbeargames
{
    public class TrapSpikes : NetworkBehaviour
    {
        public List<ZombieHealth> ListZombieVictims = new List<ZombieHealth>();
        public List<Spike> ListSpikes = new List<Spike>();

        private Coroutine SpikeTriggerRoutine;
        private bool SpikesReloaded;

        private void Start()
        {
            SpikeTriggerRoutine = null;
            SpikesReloaded = true;
            ListSpikes.Clear();
            ListZombieVictims.Clear();

            Spike[] arr = this.gameObject.GetComponentsInChildren<Spike>();
            foreach (Spike s in arr)
            {
                ListSpikes.Add(s);
            }
        }

        private void Update()
        {
            if (IsServer && ListZombieVictims.Count > 0)
            {
                foreach (ZombieHealth zombie in ListZombieVictims)
                {
                    if (zombie != null && SpikeTriggerRoutine == null && SpikesReloaded)
                    {
                        TriggerSpikes(zombie);
                    }
                }
            }
        }

        [Server]
        private void TriggerSpikes(ZombieHealth zombie)
        {
            if (zombie != null)
            {
                zombie.TakeDamage(50); // Gây 50 sát thương
                SpikeTriggerRoutine = StartCoroutine(_TriggerSpikes());
            }
        }

        [Server]
        IEnumerator _TriggerSpikes()
        {
            SpikesReloaded = false;

            // Kích hoạt các spike
            foreach (Spike s in ListSpikes)
            {
                s.Shoot();
            }

            // Gửi thông tin lên client để đồng bộ animation spike
            RpcSyncSpikeAnimation(true);

            yield return new WaitForSeconds(1.5f);

            foreach (Spike s in ListSpikes)
            {
                s.Retract();
            }

            RpcSyncSpikeAnimation(false);

            yield return new WaitForSeconds(1f);

            SpikeTriggerRoutine = null;
            SpikesReloaded = true;
        }

        [ObserversRpc]
        private void RpcSyncSpikeAnimation(bool isShooting)
        {
            foreach (Spike s in ListSpikes)
            {
                if (isShooting)
                {
                    s.Shoot();
                }
                else
                {
                    s.Retract();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            ZombieHealth zombie = other.gameObject.transform.root.gameObject.GetComponent<ZombieHealth>();
            if (zombie != null && !ListZombieVictims.Contains(zombie))
            {
                ListZombieVictims.Add(zombie);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;

            ZombieHealth zombie = other.gameObject.transform.root.gameObject.GetComponent<ZombieHealth>();
            if (zombie != null && ListZombieVictims.Contains(zombie))
            {
                ListZombieVictims.Remove(zombie);
            }
        }
    }
}