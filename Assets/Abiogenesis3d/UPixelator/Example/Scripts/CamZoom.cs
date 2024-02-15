using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class CamZoom : MonoBehaviour
{
    [HideInInspector]
    public float value;

    public float distance = 10;

    public float distanceMin = 5;
    public float distanceMax = 20;

    public float sensitivity = 10;
    public float lerpSpeed = 10;

    void Start()
    {
        value = distance;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (scrollDelta != 0)
            value -= scrollDelta * sensitivity;

        value = Mathf.Clamp(value, distanceMin, distanceMax);

        distance = Mathf.Lerp(distance, value, lerpSpeed * dt);

        // halfFrustumHeight
        // cam.orthographicSize = camZoom.distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Vector3 playerOrbitPosition = transform.position + offset; // player
        // cam.transform.position = playerOrbitPosition -cam.transform.forward * distance;
    }
}
}
