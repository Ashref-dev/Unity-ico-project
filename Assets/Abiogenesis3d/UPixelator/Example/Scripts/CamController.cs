using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
[RequireComponent(typeof(CamZoom))]
[RequireComponent(typeof(CamRotate))]
public class CamController : MonoBehaviour
{
    public Transform target;

    // NOTE: some shaders look bad when cam is too close, add extra distance only to orthographic
    public float extraOrthoOffset;

    Camera cam;
    CamZoom camZoom;
    CamRotate camRotate;

    public Vector3 offset = new Vector3(0, 1, 0);

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        camZoom = GetComponent<CamZoom>();
        camRotate = GetComponent<CamRotate>();
    }

    void LateUpdate()
    {
        Vector3 camTargetPos = target.position + offset;

        cam.transform.rotation = camRotate.value;
        cam.transform.position = camTargetPos -cam.transform.forward * camZoom.value;

        // halfFrustumHeight
        cam.orthographicSize = camZoom.value * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        if (cam.orthographic)
        {
            cam.transform.position -= cam.transform.forward * extraOrthoOffset;
        }
    }
}
}
