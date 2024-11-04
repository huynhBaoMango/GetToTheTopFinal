using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace scgFullBodyController
{
    public class CameraControlledIK : NetworkBehaviour
    {
        public Transform spineToOrientate;

        // Update is called once per frame
        void LateUpdate()
        {
            spineToOrientate.rotation = transform.rotation;
        }
    }
}
