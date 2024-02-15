using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class ReturnToTarget : MonoBehaviour
{
    public float distance = 10;
    public Transform target;

    void LateUpdate()
    {
        if (!target) return;
        if (Vector3.Distance(transform.position, target.position) > distance)
            transform.position = target.position;
    }
}
}
