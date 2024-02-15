using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class DEMO_Legsanim_JumpOnKey : MonoBehaviour
    {
        public Rigidbody ToJump;
        public Vector3 JumpVector = new Vector3(0, 5f);
        public KeyCode Key = KeyCode.Space;

        private void Start()
        {
            if (ToJump == null) ToJump = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (ToJump == null) return;

            if (Input.GetKeyDown(Key)) { ToJump.position = ToJump.position + JumpVector * 0.01f; ToJump.velocity = JumpVector; }
        }

    }
}