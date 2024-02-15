using FIMSpace.FProceduralAnimation;
using UnityEngine;

public class DEMO_LegsAnim_LASwitcher : MonoBehaviour
{
    public LegsAnimator Switch;
    public void SwitchLegsAnimator()
    {
        Switch.enabled = !Switch.enabled;
    }
}
