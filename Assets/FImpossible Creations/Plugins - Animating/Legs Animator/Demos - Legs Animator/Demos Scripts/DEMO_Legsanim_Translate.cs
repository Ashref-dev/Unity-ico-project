using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_Legsanim_Translate : MonoBehaviour
    {
        public Vector3 LocalOffset = Vector3.zero;
        Rigidbody rig;

        private void Start()
        {
            rig = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (rig != null) return;
            transform.position += transform.TransformVector(LocalOffset * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (rig == null) return;
            Vector3 newVelo = transform.TransformVector(LocalOffset);
            newVelo.y = rig.velocity.y;
            rig.velocity = newVelo;
        }
    }
}