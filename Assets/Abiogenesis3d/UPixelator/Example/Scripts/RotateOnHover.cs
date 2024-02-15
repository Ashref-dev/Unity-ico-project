using UnityEngine;

namespace Abiogenesis3d
{
    public class RotateOnHover : MonoBehaviour
    {
        bool isRotating;

        void Update()
        {
            if (isRotating)
            {
                transform.Rotate(Vector3.up, 90 * Time.deltaTime);
            }
        }

        void OnMouseEnter()
        {
            isRotating = true;
        }

        void OnMouseExit()
        {
            isRotating = false;
        }
    }
}
