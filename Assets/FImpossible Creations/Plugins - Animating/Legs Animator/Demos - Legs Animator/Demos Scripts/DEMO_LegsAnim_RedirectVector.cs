using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMO_LegsAnim_RedirectVector : MonoBehaviour
{
    public LegsAnimator Legs;
    public Vector3 Dir = Vector3.zero;

    void Start()
    {
            
    }

    void Update()
    {
        Legs.SetCustomIKRotatorVector( Legs.transform.rotation * Dir);
        Legs.User_UpdateParametersAfterManualChange();
    }
}
