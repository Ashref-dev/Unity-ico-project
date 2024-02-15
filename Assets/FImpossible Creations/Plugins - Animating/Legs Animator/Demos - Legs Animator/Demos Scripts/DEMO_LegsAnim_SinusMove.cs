using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_SinusMove : MonoBehaviour
    {
        public Vector3 Offset = Vector3.right;
        public float Speed = 1f;

        Vector3 startPos;
        private void Start()
        {
            startPos = transform.position;
        }

        float elapsed = 0f;
        void Update()
        {
            elapsed += Time.deltaTime * Speed;
            transform.position = startPos + Offset * Mathf.Sin(elapsed);
        }

    }
}