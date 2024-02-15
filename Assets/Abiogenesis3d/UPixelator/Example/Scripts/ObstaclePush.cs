using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class ObstaclePush : MonoBehaviour
{
    public float forceMagnitude = 1;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null) return;

        Vector3 forceDirection = hit.gameObject.transform.position - transform.position;
        forceDirection.y = 0;
        forceDirection.Normalize();

        rb.AddForceAtPosition(forceDirection * forceMagnitude, transform.position, ForceMode.Impulse);
    }
}
}
