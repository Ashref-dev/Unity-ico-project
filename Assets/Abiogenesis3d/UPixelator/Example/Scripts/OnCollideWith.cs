using UnityEngine;
using UnityEngine.Events;

namespace Abiogenesis3d.UPixelator_Demo
{
public class OnCollideWith : MonoBehaviour
{
    public Transform target;
    public UnityEvent onCollideWith;

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
            if (collision.transform == target)
                onCollideWith.Invoke();
    }
}
}