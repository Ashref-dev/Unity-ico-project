using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class LookAtY : MonoBehaviour
{
    public Transform target;
    public float maxAngle = 90;

    void Update()
    {
        Vector3 lookDirection = transform.position - target.position;
        Quaternion lookAtRotation = Quaternion.LookRotation(lookDirection);
        Vector3 angles = lookAtRotation.eulerAngles;
        angles.z = 0; // prevent tilt
        transform.rotation = Quaternion.Euler(angles);
    }
}
}
