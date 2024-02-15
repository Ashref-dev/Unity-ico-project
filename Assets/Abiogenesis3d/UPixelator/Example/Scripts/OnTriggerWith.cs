using UnityEngine;
using UnityEngine.Events;

namespace Abiogenesis3d.UPixelator_Demo
{
public class OnTriggerWith : MonoBehaviour
{
    public Transform target;
    public UnityEvent ontTriggerWith;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform == target)
            ontTriggerWith.Invoke();
    }
}
}
