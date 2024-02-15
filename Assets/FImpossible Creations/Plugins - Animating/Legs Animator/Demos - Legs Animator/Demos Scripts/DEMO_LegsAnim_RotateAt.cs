using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_RotateAt : MonoBehaviour
    {
        public Transform ToRotate;
        public Transform LookAt;

        void Update()
        {
            Vector3 targetPos = LookAt.position;
            targetPos.y = ToRotate.position.y;
            ToRotate.LookAt(targetPos);
        }
    }
}