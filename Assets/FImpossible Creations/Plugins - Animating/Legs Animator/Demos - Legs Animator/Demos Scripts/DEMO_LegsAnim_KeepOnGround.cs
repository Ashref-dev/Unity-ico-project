using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_KeepOnGround : MonoBehaviour
    {
        public LayerMask mask;
        public float raycastRange = 0.1f;
        Rigidbody rig;
        private void Start()
        {
            rig = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rig == null) return;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * raycastRange * 0.5f, Vector3.down, out hit, raycastRange * 0.5f + raycastRange, mask, QueryTriggerInteraction.Ignore))
            {
                rig.MovePosition(hit.point);
            }
        }
    }
}