using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_SwitchMultiple : MonoBehaviour
    {
        public List<LegsAnimator> legsAnims;
        public void SwitchEnable(bool enable)
        {
            for (int i = 0; i < legsAnims.Count; ++i)
            {
                legsAnims[i].enabled = enable;
            }
        }
    }
}