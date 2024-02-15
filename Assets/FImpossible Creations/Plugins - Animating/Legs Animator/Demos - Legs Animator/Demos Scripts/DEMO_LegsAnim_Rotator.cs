using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_Rotator : MonoBehaviour
    {
        public Vector3 RotationSpeed = Vector3.zero;
        Rigidbody rig;
        private void Start()
        {
            rig = GetComponent<Rigidbody>();
        }


        void Update()
        {
            if (rig != null) return;
            transform.Rotate(RotationSpeed * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (rig == null) return;
            if (rig.isKinematic)
                rig.rotation *= Quaternion.Euler(RotationSpeed * Mathf.Deg2Rad);
            else
                rig.angularVelocity = RotationSpeed * Mathf.Deg2Rad;
        }
    }
}