using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class CamRotate : MonoBehaviour
{
    [HideInInspector]
    public Quaternion value;

    Camera cam;

    Vector3 eulerAngles;

    public float minAngleX = 10;
    public float maxAngleX = 89;

    public float rotationSpeed = 200;

    // TODO: move to module
    public Vector2 mousePosition;

    void Start()
    {
        cam = Camera.main;

        eulerAngles = cam.transform.eulerAngles;
        Rotate();
    }

    void Update()
    {

        if (Input.GetMouseButton(1))
        {
            Rotate();
        }
        else
        {
            Cursor.visible = true;
            mousePosition = Input.mousePosition;
        }
    }

    void Rotate()
    {
        float dt = Time.deltaTime;

        eulerAngles.y += Input.GetAxis("Mouse X") * rotationSpeed * dt;
        eulerAngles.x -= Input.GetAxis("Mouse Y") * rotationSpeed * dt;

        eulerAngles.x = ClampAngle(eulerAngles.x, minAngleX, maxAngleX);

        value = Quaternion.Euler(eulerAngles);
    }

    public static float ClampAngle(float angle, float min, float max) {
        return Mathf.Clamp(angle % 360, min, max);
    }
}
}
