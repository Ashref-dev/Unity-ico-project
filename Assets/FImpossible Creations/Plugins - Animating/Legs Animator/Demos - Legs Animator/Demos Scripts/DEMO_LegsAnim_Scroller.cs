using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_LegsAnim_Scroller : MonoBehaviour
    {
        public Vector3 MoveDirection = Vector3.zero;
        public float RestartBelowX = -3f;
        public float MoveBackBy = 6f;


        void Update()
        {
            if (transform.position.x < RestartBelowX) transform.position -= MoveDirection.normalized * MoveBackBy;
            transform.position += MoveDirection * Time.deltaTime;
        }

    }
}