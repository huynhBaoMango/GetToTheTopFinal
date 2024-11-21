using System.Collections;
using UnityEngine;

namespace Roundbeargames
{
    public class Spike : MonoBehaviour
    {
        public void Shoot()
        {
            if (this.transform.localPosition.y < 0f)
            {
                StartCoroutine(_Shoot());
            }
        }

        IEnumerator _Shoot()
        {
            // Lo?i b? delay ng?u nhiên
            yield return null; // Không delay
            this.transform.localPosition += (Vector3.up * 1f);
        }

        public void Retract()
        {
            if (this.transform.localPosition.y > 0f)
            {
                StartCoroutine(_Retract());
            }
        }

        IEnumerator _Retract()
        {
            // Lo?i b? delay ng?u nhiên
            yield return null; // Không delay
            this.transform.localPosition -= (Vector3.up * 1f);
        }
    }
}
