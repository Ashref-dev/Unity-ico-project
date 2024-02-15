using FIMSpace.Basics;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    public class DEMO_LegsAnim_Mover : MonoBehaviour
    {
        public bool Move;
        public Fimp_JoystickInput JoystickInput;
        public Rigidbody Rigb;
        [Header("Setting 'IsGrounded','IsMoving' and 'Speed' parameters for mecanim")]
        public Animator Mecanim;
        public bool StrafeMode = false;
        [Space(4)]
        public LegsAnimator AutoSetGroundedAndIsMoving = null;
        [Space(4)]
        public float MovementSpeed = 2f;
        [Range(0f, 1f)]
        public float RotateToSpeed = 0.8f;
        public bool AutoRotation = true;

        [Range(0f, 1f)] public float DirectMovement = 0f;

        [Space(4)]
        public LayerMask GroundMask = 0 >> 1;
        [Space(4)]
        public float JumpPower = 3f;
        public float ExtraRaycastDistance = 0f;

        [Space(4)]
        public float HoldShiftForSpeed = 0f;
        public float HoldCtrlForSpeed = 0f;

        Quaternion targetRotation;
        Quaternion targetInstantRotation;
        bool isGrounded = true;

        [Space(4)]
        public LegsAnimator CallImpulseOn;
        public LegsAnimator.PelvisImpulseSettings ImpulseBeforeJump;

        [Space(4)]
        public string SetMecanimTrigger = "";
        public KeyCode MecanimTriggerOnKey = KeyCode.Q;

        void Start()
        {
            if (!Rigb) Rigb = GetComponent<Rigidbody>();
            if (Rigb)
            {
                Rigb.maxAngularVelocity = 30f;
                if (Rigb.interpolation == RigidbodyInterpolation.None) Rigb.interpolation = RigidbodyInterpolation.Interpolate;
                Rigb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            targetRotation = transform.rotation;
            targetInstantRotation = transform.rotation;
            rotationAngle = transform.eulerAngles.y;

            if (Mecanim) Mecanim.SetBool("IsGrounded", true);
            if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsGrounded(true);
        }

        Vector2 moveDirectionLocal;
        Vector2 moveDirectionLocalNonZero;
        Vector3 moveDirectionWorld;
        float rotationAngle = 0f;
        float sd_rotationAngle = 0f;

        float toJump = 0f;

        void Update()
        {
            if (Rigb == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (toJump <= 0f)
                {
                    jumpRequest = JumpPower;

                    if (CallImpulseOn != null)
                    {
                        toJump = ImpulseBeforeJump.ImpulseDuration * 0.6f;
                        CallImpulseOn.User_AddImpulse(ImpulseBeforeJump);
                    }
                    else
                    {
                        toJump = 0f;
                    }
                }
            }
            
                moveDirectionLocal = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) moveDirectionLocal += Vector2.left;
            else if (Input.GetKey(KeyCode.D)) moveDirectionLocal += Vector2.right;

            if (Input.GetKey(KeyCode.W)) moveDirectionLocal += Vector2.up;
            else if (Input.GetKey(KeyCode.S)) moveDirectionLocal += Vector2.down;

            if (JoystickInput)
            if (JoystickInput.OutputValue != Vector2.zero)
            {
                moveDirectionLocal.x += JoystickInput.OutputValue.x;
                moveDirectionLocal.y += JoystickInput.OutputValue.y;
            }

            bool moving = false;

            
            Quaternion flatCamRot = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);

            if (moveDirectionLocal != Vector2.zero)
            {
                moveDirectionLocal.Normalize();
                moveDirectionWorld = flatCamRot * new Vector3(moveDirectionLocal.x, 0f, moveDirectionLocal.y);

                moving = true;
                if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsMoving(true);

                moveDirectionLocalNonZero = moveDirectionLocal;
            }
            else
            {
                if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsMoving(false);
                moveDirectionWorld = Vector3.zero;
            }

            if (Input.GetKey(KeyCode.R) || moveDirectionLocal != Vector2.zero)
            {
                if (RotateToSpeed > 0f)
                    if (currentWorldAccel != Vector3.zero)
                    {
                        //! ai trigger to auto move and control rotation by another script
                        if (Move)
                        {
                        targetInstantRotation = StrafeMode ? flatCamRot : Quaternion.LookRotation(currentWorldAccel);

                        rotationAngle = Mathf.SmoothDampAngle(rotationAngle, targetInstantRotation.eulerAngles.y, ref sd_rotationAngle, Mathf.Lerp(0.5f, 0.01f, RotateToSpeed));
                        targetRotation = Quaternion.Euler(0f, rotationAngle, 0f);// Quaternion.RotateTowards(targetRotation, targetInstantRotation, Time.deltaTime * 90f * RotateToSpeed);
                        }
                    }
            }

            if (Mecanim) Mecanim.SetBool("IsMoving", moving);

            float spd = MovementSpeed;
            if (HoldShiftForSpeed != 0f) if (Input.GetKey(KeyCode.LeftShift)) spd = HoldShiftForSpeed;
            if (HoldCtrlForSpeed != 0f) if (Input.GetKey(KeyCode.LeftControl)) spd = HoldCtrlForSpeed;

            float accel = 5f * MovementSpeed;
            if (!moving) accel = 7f * MovementSpeed;

            currentWorldAccel = Vector3.MoveTowards(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel);
            if (Mecanim) if (moving) Mecanim.SetFloat("Speed", currentWorldAccel.magnitude);

            if (Mecanim) if (!string.IsNullOrWhiteSpace(SetMecanimTrigger)) if (Input.GetKeyDown(MecanimTriggerOnKey)) Mecanim.SetTrigger(SetMecanimTrigger);
        }


        Vector3 currentWorldAccel = Vector3.zero;

        float jumpRequest = 0f;
        private void FixedUpdate()
        {
            if (Rigb == null) return;

            //Vector3 localAccel = transform.InverseTransformDirection(currentWorldAccel);
            //Vector3 localVelo = transform.InverseTransformDirection(Rigb.velocity);
            //localVelo.x = localAccel.x;
            //localVelo.z = localAccel.z;
            //localVelo.y = 0f;

            Vector3 targetVelo = currentWorldAccel;

            float yAngleDiff = Mathf.DeltaAngle(Rigb.rotation.eulerAngles.y, targetInstantRotation.eulerAngles.y);
            float directMovement = DirectMovement;
            directMovement *= Mathf.InverseLerp(180f, 50f, Mathf.Abs(yAngleDiff));
            targetVelo = Vector3.Lerp(targetVelo, (StrafeMode ? transform.rotation * new Vector3(moveDirectionLocalNonZero.x, 0f, moveDirectionLocalNonZero.y) : transform.forward) * targetVelo.magnitude, directMovement);
            targetVelo.y = Rigb.velocity.y;

            toJump -= Time.fixedDeltaTime;

            if (jumpRequest != 0f && toJump <= 0f)
            {
                Rigb.position += transform.up * jumpRequest * 0.01f;
                targetVelo.y = jumpRequest;
                isGrounded = false;
                jumpRequest = 0f;
                jumpTime = Time.time;
                if (Mecanim) Mecanim.SetBool("IsGrounded", false);
                if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsGrounded(false);
            }
            else
            {
                if (isGrounded)
                {
                    targetVelo.y -= 2.5f * Time.fixedDeltaTime;
                }
            }

            Rigb.velocity = targetVelo;
            Rigb.angularVelocity = FEngineering.QToAngularVelocity(Rigb.rotation, targetRotation, true);

            if (Time.time - jumpTime > 0.2f)
            {
                //float radius = 0.3f;
                //if (Physics.SphereCast(new Ray(transform.position + transform.up, -transform.up), radius,   1.01f - radius, GroundMask, QueryTriggerInteraction.Ignore))
                if (Physics.Raycast(transform.position + transform.up, -transform.up, (isGrounded ? 1.2f : 1.01f) + ExtraRaycastDistance, GroundMask, QueryTriggerInteraction.Ignore))
                {
                    if (isGrounded == false)
                    {
                        isGrounded = true;
                        if (Mecanim) Mecanim.SetBool("IsGrounded", true);
                        if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsGrounded(true);
                    }
                }
                else
                {
                    if (isGrounded == true)
                    {
                        isGrounded = false;
                        if (Mecanim) Mecanim.SetBool("IsGrounded", false);
                        if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsGrounded(false);
                    }
                }
            }
            else
            {
                if (isGrounded == true)
                {
                    isGrounded = false;
                    if (Mecanim) Mecanim.SetBool("IsGrounded", false);
                    if (AutoSetGroundedAndIsMoving) AutoSetGroundedAndIsMoving.User_SetIsGrounded(false);
                }
            }

        }

        float jumpTime = -1f;


        public void SwitchStrafeMode()
        {
            StrafeMode = !StrafeMode;
        }
    }
}