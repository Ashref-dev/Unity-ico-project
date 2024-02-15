using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_MoveLegTowards : MonoBehaviour
    {
        public LegsAnimator LegsAnim;
        public int LegIndex = 0;
        public Transform Target;

        [Space(5)]
        public bool Apply = true;
        
        public void SwitchUse()
        {
            Apply = !Apply;
        }

        void Update()
        {
            if (LegsAnim == null) return;
            if (Target == null ) Apply = false;

            if (!Apply)
            {
                LegsAnim.User_MoveLegTo_Restore(LegIndex);
                return;
            }

            LegsAnim.User_MoveLegTo(LegIndex, Target);
        }
    }
}