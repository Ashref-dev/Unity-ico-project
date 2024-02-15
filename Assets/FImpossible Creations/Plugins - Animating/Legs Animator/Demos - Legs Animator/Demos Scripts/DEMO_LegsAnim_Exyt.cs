using FIMSpace.Basics;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    public class DEMO_LegsAnim_Exyt : MonoBehaviour
    {
        public LegsAnimator LegsAnimator;


        //void j()
        //{

        //    if (LegsAnimator == null) return;

        //    if (LegsAnimator.RedirectHips > 0f)
        //    {
        //        if (moveDirectionLocal == Vector2.zero)
        //        {
        //            //SetIKOffsetsRotate(0f, IKRotateTransitioningDur <= 0f ? 0f : 0.3f);
        //            LegsAnimator.IKOffsetsRotate = 0f;
        //        }
        //        else
        //        {
        //            float targetAngle = LegsAnimator.User_GetLocalRotationAngle(moveDirectionWorld, transform.forward);
        //            LegsAnimator.IKOffsetsRotate = targetAngle;
        //            //SetIKOffsetsRotate(targetAngle, IKRotateTransitioningDur);
        //        }

        //        LegsAnimator.User_UpdateParametersAfterManualChange();
        //    }
        //}

        //float _sd_ikAngle = 0f;
        //void SetIKOffsetsRotate(float val, float duration = 0.1f) 
        //{
        //    LegsAnimator._CustomTargetedIKOffsetsRotate = val;

        //    //if (val < -90f)
        //    //    val = Mathf.Lerp(-90f, 0f, Mathf.InverseLerp(-90f, -180f, val));
        //    //else if (val > 90f)
        //    //    val = Mathf.Lerp(90f, 0f, Mathf.InverseLerp(90f, 180f, val));

        //    //if ( val == 0f)
        //    //{
        //    //    float angle = LegsAnimator.IKOffsetsRotate;
        //    //    angle = angle % 360f;
        //    //    if (angle > 180f) angle -= 360f;
        //    //    if (angle < -180f) angle += 360f;

        //    //    LegsAnimator.IKOffsetsRotate = angle;

        //    //    LegsAnimator.IKOffsetsRotate =
        //    //        Mathf.SmoothDamp(LegsAnimator.IKOffsetsRotate, val, ref _sd_ikAngle, duration, float.MaxValue, Time.deltaTime);

        //    //    return;
        //    //}

        //    if ( duration <= 0f)
        //    {
        //        LegsAnimator.IKOffsetsRotate = val;
        //        return;
        //    }

        //    LegsAnimator.IKOffsetsRotate =
        //        Mathf.SmoothDampAngle(LegsAnimator.IKOffsetsRotate, val, ref _sd_ikAngle, duration, float.MaxValue, Time.deltaTime);

        //    //LegsAnimator.IKOffsetsRotate %= 180f;
        //}

    }
}