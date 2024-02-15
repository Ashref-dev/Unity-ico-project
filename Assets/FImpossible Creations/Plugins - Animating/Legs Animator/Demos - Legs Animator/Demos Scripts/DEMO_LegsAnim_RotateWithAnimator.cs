using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_RotateWithAnimator : MonoBehaviour
    {
        public Animator animator;
        public string AnimatorParameter;
        public Vector3 RotationSpeed = Vector3.zero;
        public Transform ToRotate;

        float rotSpd = 0f;
        float _sd = 0f;

        float rotated = 0f;

        void LateUpdate()
        {
            if (animator.GetBool(AnimatorParameter))
                rotSpd = Mathf.SmoothDamp(rotSpd, 1f, ref _sd, 0.2f, float.MaxValue, Time.deltaTime);
            else
                rotSpd = Mathf.SmoothDamp(rotSpd, 0f, ref _sd, 0.35f, float.MaxValue, Time.deltaTime);

            rotated += rotSpd * Time.deltaTime;

            ToRotate.localRotation = Quaternion.Euler(RotationSpeed * rotated);
        }

    }
}