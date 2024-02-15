using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class FollowDelayed : MonoBehaviour
{
    public Transform target;
    public float speed = 10;

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }
}
}
