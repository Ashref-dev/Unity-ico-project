using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class OnEventTeleport : MonoBehaviour
{
    public Transform target;
    public Transform destination;
    public float delay;

    bool shouldTeleport;
    float eventTime;

    void Update()
    {
        if (!shouldTeleport) return;

        if (Time.time - eventTime >= delay)
            Teleport();
    }

    public void OnEvent()
    {
        shouldTeleport = true;
        eventTime = Time.time;
    }

    void Teleport()
    {
        target.position = destination.position;
        shouldTeleport = false;
    }
}
}
