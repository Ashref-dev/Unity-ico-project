using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnimScenesSwitches : MonoBehaviour
    {
        public LegsAnimator legsAnim;

        float initStretchPreventer;
        private void Start()
        {
            if (!legsAnim) return;
            initStretchPreventer = legsAnim.HipsStretchPreventer;
        }

        public void SwitchSlowmo()
        {
            if (Time.timeScale == 1f) Time.timeScale = 0.6f; else Time.timeScale = 1f;
        }

        public void SwitchStepSmooth(bool on)
        {
            if (on) legsAnim.SmoothSuddenSteps = 1f; else legsAnim.SmoothSuddenSteps = 0f;
        }

        public void SwitchLegElevate(bool on)
        {
            if (on) legsAnim.LegElevateBlend = 2f; else legsAnim.LegElevateBlend = 0f;
        }

        public void SwitchStability(bool on)
        {
            if (on) legsAnim.StabilizeCenterOfMass = 1f; else legsAnim.StabilizeCenterOfMass = 0f;
        }

        public void SwitchStretchPreventer(bool on)
        {
            if (on) legsAnim.HipsStretchPreventer = initStretchPreventer; else legsAnim.HipsStretchPreventer = 0f;
        }

        public void SwitchGluing(bool on)
        {
            if (on) legsAnim.UseGluing = true; else legsAnim.UseGluing = false;
        }

    }
}