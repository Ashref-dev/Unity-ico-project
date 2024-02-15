using FIMSpace.FProceduralAnimation;
using UnityEngine;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

public class DEMO_LegsAnim_StepEvents : MonoBehaviour, LegsAnimator.ILegStepReceiver
{
    public AudioSource StepSource;
    public AudioClip[] StepClips;
    public AudioClip[] LandClips;

    [Space(4)]
    public GameObject Particle;

    public void PlayStepAudio(float volumeMul = 1f)
    {
        if (StepSource == null) return;
        if (StepClips.Length == 0) return;
        StepSource.PlayOneShot(StepClips[Random.Range(0, StepClips.Length)], volumeMul);
    }

    public void PlayLandAudio(float volumeMul = 1f)
    {
        if (StepSource == null) return;
        if (LandClips.Length == 0) return;
        StepSource.PlayOneShot(LandClips[Random.Range(0, LandClips.Length)], volumeMul);
    }

    public void LegAnimatorStepEvent(LegsAnimator.Leg leg, float power, bool isRight, Vector3 position, Quaternion rotation, LegsAnimator.EStepType type)
    {
        if (Particle != null)
        {
            GameObject particle = Instantiate(Particle);

            if (type == EStepType.OnLanding)
            {
                particle.transform.position = leg.Owner.BaseTransform.position;
                particle.transform.localScale = Particle.transform.localScale * 1.65f;
            }
            else
                particle.transform.position = position;

            particle.transform.rotation = rotation * Quaternion.Euler(-90f, -90f, 0f);
        }

        if (type == EStepType.OnLanding /*|| type == EStepType.OnStopping*/)
            PlayLandAudio(Mathf.Lerp(0.75f, 1f, power));
        else
            PlayStepAudio(Mathf.Lerp(0.5f, 1f, power));
    }
}
