using FIMSpace.Basics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    public class DEMO_LegsAnim_SpiderMover : MonoBehaviour
    {
        public Fimp_JoystickInput JoystickInput;
        public Transform ToRotate;
        public Transform ToOffset;
        public Animator Mecanim;
        public LegsAnimator ToAssignHelperVars;

        [Header("Movement")]
        public float MovementSpeed = 2f;
        public float SpeedOnShift = 3f;
        [Range(0f, 1f)] public float RotateToSpeed = .75f;
        [Space(4)]
        public LayerMask GroundMask = 0 >> 1;
        [Space(4)]
        public float JumpPower = 3f;
        [Space(4)]
        [Range(0f, 1f)] public float GroundAlignSpeed = 0.8f;
        [Range(0f, 2f)] public float GravityPower = 1f;

        [Header("Control")]
        [Range(0f, 360f)]
        public float UpAxisAngle = 0f;
        public bool StrafeMode = false;


        [Header("Raycasting Setup")]
        public Vector3 OriginOffset = Vector3.up;

        [Space(3)]
        [Range(0.05f, 1f)] public float FirstRayDistance = 1f;
        [Range(0.05f, 1f)] public float SecondRayDistance = 1f;

        [Space(5)]
        [Range(0f, 90f)] public float FirstAngle = 45f;
        [Range(0f, 90f)] public float SecondAngle = 45f;

        [Space(3)]
        [Range(0f, 1f)] public float SecondCastAlong = 0.75f;
        [Range(0f, 2f)] public float CollapseSides = 0f;
        [Range(0f, 1f)] public float HitMemoryDistance = 0.1f;
        [Range(-1f, 1f)] public float CounterOffsets = 0.1f;
        [Range(0f, 90f)] public float SkipSimilar = 0f;


        Quaternion raycastRotation = Quaternion.identity;
        Quaternion modelRotation = Quaternion.identity;
        Vector3 rotationNormal = Vector3.up;
        Vector3 raycastingRNormal = Vector3.up;
        Vector3 sd_rotNorm = Vector3.zero;

        Vector3 raycastNormal = Vector3.up;
        Vector3 sd_castNorm = Vector3.zero;
        float sd_upAxis = 0f;

        Vector2 moveDirectionLocal = Vector3.zero;
        Vector3 moveDirectionWorld = Vector3.zero;
        float acceleration = 0f;
        float sd_accel = 0f;

        float lastTargetAngle = 0f;
        float jumpRequest = 0f;
        //bool isJumping = false;
        float jumpCulldown = 0f;
        bool isGrounded = true;
        //Vector3 closestToOriginGroundHit = Vector3.zero;
        //Vector3 closestToOriginGroundHitLocal = Vector3.zero;

        FDebug_PerformanceTest perf = new FDebug_PerformanceTest();
        //Matrix4x4 raycastMx;
        //Matrix4x4 raycastMxInv;
        Vector3 raycastUp = Vector3.up;
        float jumpIn = 0f;
        public LegsAnimator.PelvisImpulseSettings ToJumpImpulse;

        void Start()
        {
            if (ToRotate == null) ToRotate = transform;
            raycastRotation = transform.rotation;
            UpAxisAngle = transform.eulerAngles.y;
            ResetSpidercasters();
            rotationNormal = transform.up;
            raycastNormal = transform.up;
            lastTargetAngle = transform.eulerAngles.y;
        }

        Vector3 TransformPoint(Vector3 pos)
        {
            return transform.TransformPoint(pos);
            //return raycastMx.MultiplyPoint3x4(pos);
        }

        Vector3 InversePoint(Vector3 pos)
        {
            return transform.InverseTransformPoint(pos);
            //return raycastMxInv.MultiplyPoint3x4(pos);
        }

        Vector3[] applied = new Vector3[16];
        int appliedCount = 0;

        void Update()
        {
            perf.Start(gameObject);

            //raycastMx = Matrix4x4.TRS(transform.position, raycastRotation, transform.lossyScale);
            //raycastMxInv = raycastMx.inverse;


            #region Input Update

            jumpIn -= Time.deltaTime;
            if (isGrounded && jumpIn <= 0f)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (ToAssignHelperVars)
                    {
                        jumpIn = 0.2f;
                        ToAssignHelperVars.User_AddImpulse(ToJumpImpulse);
                    }

                    jumpRequest = JumpPower;
                }
            }

            moveDirectionLocal = Vector2.zero;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDirectionLocal += Vector2.left;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDirectionLocal += Vector2.right;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDirectionLocal += Vector2.up;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveDirectionLocal += Vector2.down;

            if (JoystickInput.OutputValue != Vector2.zero)
            {
                moveDirectionLocal.x += JoystickInput.OutputValue.x;
                moveDirectionLocal.y += JoystickInput.OutputValue.y;
            }

            #endregion


            #region Main Movement Control Apply

            if (moveDirectionLocal != Vector2.zero)
            {
                moveDirectionLocal.Normalize();

                Quaternion flatCamRot = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);

                moveDirectionWorld = flatCamRot * new Vector3(moveDirectionLocal.x, 0f, moveDirectionLocal.y);

                lastTargetAngle = FEngineering.GetAngleDeg(moveDirectionWorld.x, moveDirectionWorld.z);

                if (Mecanim) Mecanim.SetBool("IsMoving", true);
                if (ToAssignHelperVars) ToAssignHelperVars.User_SetIsMoving(true);

                acceleration = Mathf.SmoothDamp(acceleration, 1f, ref sd_accel, 0.1f, 10000f, Time.deltaTime);
            }
            else
            {
                if (Mecanim) Mecanim.SetBool("IsMoving", false);
                if (ToAssignHelperVars) ToAssignHelperVars.User_SetIsMoving(false);

                acceleration = Mathf.SmoothDamp(acceleration, 0f, ref sd_accel, 0.04f, 10000f, Time.deltaTime);
            }

            float tgtAngle = Mathf.LerpAngle(UpAxisAngle, lastTargetAngle, acceleration);

            UpAxisAngle = Mathf.SmoothDampAngle(UpAxisAngle, tgtAngle, ref sd_upAxis, Mathf.Lerp(0.6f, 0.0005f, RotateToSpeed), 1000f + RotateToSpeed * 9000f, Time.deltaTime);

            if (ToAssignHelperVars) ToAssignHelperVars.User_SetDesiredMovementDirection(moveDirectionWorld);

            #endregion



            #region Physics Call

            ProceedSpidercasts();
            ReInitializeRaycastCounterPool(spiderCasters.Length * 2);

            Vector3 averageNormal = Vector3.zero;
            isGrounded = false;
            //closestToOriginGroundHitLocal = Vector3.zero;
            lowestLocal.y = float.MaxValue;
            highestLocal.y = float.MinValue;

            for (int s = 0; s < spiderCasters.Length; s++) // Check all spidercasters
            {
                if (spiderCasters[s].anyHit)
                {
                    AnalyzeRaycast(spiderCasters[s].firstHit, ref averageNormal, spiderCasters[s], true);
                    AnalyzeRaycast(spiderCasters[s].secondaryHit, ref averageNormal, spiderCasters[s], false);
                }
            }

            #endregion



            #region Use physics result to calculate rotations


            if (averageNormal == Vector3.zero) averageNormal = Vector3.up;
            else averageNormal.Normalize();

            Vector3 targetModelRotNormal = Vector3.zero;

            appliedCount = 0;

            if (SkipSimilar < 0.1f)
            {
                for (int i = 0; i < currentNormalIDs; i++)
                {
                    targetModelRotNormal += raycastCounters[i].normal;
                }

                targetModelRotNormal.Normalize();
            }
            else
            {
                for (int i = 0; i < currentNormalIDs; i++)
                {
                    bool can = true;
                    for (int a = 0; a < appliedCount; a++)
                    {
                        float angle = Vector3.Angle(applied[a], raycastCounters[i].normal);
                        if (angle < SkipSimilar) { can = false; break; }
                    }

                    if (can)
                    {
                        targetModelRotNormal += raycastCounters[i].normal;
                        applied[appliedCount] = raycastCounters[i].normal;
                        appliedCount += 1;
                    }

                    //for (int c = 0; c < raycastCounters[i].count; c++) movementRotNormal += raycastCounters[i].normal;
                }

                targetModelRotNormal.Normalize();
            }


            raycastRotation = Quaternion.FromToRotation(Vector3.up, targetModelRotNormal); //targetModelRotNormal.Normalize(); 
            raycastRotation = raycastRotation * Quaternion.AngleAxis(UpAxisAngle, Vector3.up); // Instant Rot 
            raycastUp = raycastRotation * Vector3.up;


            float spd = Mathf.Lerp(4f, 24f, GroundAlignSpeed);

            if (!isGrounded)
                rotationNormal = Vector3.Slerp(rotationNormal, Vector3.up, Time.deltaTime * spd * 0.5f);
            else
                rotationNormal = Vector3.Slerp(rotationNormal, targetModelRotNormal, Time.deltaTime * spd * 1.25f);

            modelRotation = Quaternion.FromToRotation(Vector3.up, rotationNormal);
            modelRotation = modelRotation * Quaternion.AngleAxis(UpAxisAngle, Vector3.up);

            //raycastingRNormal = Vector3.Slerp(raycastingRNormal, targetModelRotNormal, Time.deltaTime * spd * 8f);


            #endregion


            ToRotate.rotation = modelRotation;


            #region After rotations do movement


            Vector3 newPos = transform.position;

            if (highestLocal.y > 0f) // if detected hit above foot level (overlapping into ground?)
            {
                Vector3 newToLocal = InversePoint(newPos);
                newToLocal.y = Mathf.Lerp(newToLocal.y, highestLocal.y, Time.deltaTime * 10f);
                newPos = TransformPoint(newToLocal);
            }

            if (lowestLocal.y < 0f) // if detected hit below foot level (floating over ground?)
            {
                Vector3 newToLocal = InversePoint(newPos);
                newToLocal.y = Mathf.Lerp(newToLocal.y, lowestLocal.y, Time.deltaTime * 10f);
                newPos = TransformPoint(newToLocal);
            }

            if (isGrounded)
            {
                gravityVelo = Vector3.zero;
                if (ToOffset) ToOffset.localPosition = Vector3.SmoothDamp(ToOffset.localPosition, CalculateTargetPositionLocal(), ref sd_targetOffset, 0.05f, 10000f, Time.deltaTime);
            }
            else
            {
                if (ToOffset) ToOffset.localPosition = Vector3.SmoothDamp(ToOffset.localPosition, Vector3.zero, ref sd_targetOffset, 0.035f, 10000f, Time.deltaTime);

                gravityVelo.y += Time.deltaTime * Physics.gravity.y;
                gravityVelo.x = Mathf.Lerp(gravityVelo.x, 0f, Time.deltaTime * 2.2f);
                gravityVelo.z = Mathf.Lerp(gravityVelo.z, 0f, Time.deltaTime * 2.2f);

                newPos += Time.deltaTime * GravityPower * gravityVelo;
            }

            Vector3 targetOffset = raycastRotation * Vector3.forward;

            float mspd = MovementSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) mspd = SpeedOnShift; 
            newPos += targetOffset * Time.deltaTime * acceleration * mspd;


            #endregion

            jumpCulldown -= Time.deltaTime;

            bool jumpWait = false;
            if ( ToAssignHelperVars)
            {
                if ( jumpIn > 0f)
                {
                    jumpWait = true;
                }
            }

            if (!jumpWait)
            if (jumpRequest != 0f)
            {
                Jump(jumpRequest);
                jumpRequest = 0f;
                newPos += raycastUp * (GroundingCatchRange);
            }


            transform.position = newPos;

            perf.Finish();
        }


        public void Jump(float power)
        {
            gravityVelo = Vector3.Lerp(Vector3.up, raycastUp, 0.7f) * power;
            //isJumping = true;
            SetGrounded(false);
            jumpCulldown = 0.6f;
        }


        Vector3 gravityVelo = Vector3.zero;
        Vector3 sd_targetOffset = Vector3.zero;

        Vector3 CalculateTargetPositionLocal()
        {
            Vector3 raycastingPosition = transform.position;
            Vector3 averagePosition = Vector3.zero;
            float count = 0f;

            for (int i = 0; i < spiderCasters.Length; i++)
            {
                var caster = spiderCasters[i];
                if (caster.anyHit == false) continue;
                //if (caster.lastDetectedSec.distance != 0f)  { averagePosition += caster.lastSecHitLocal; count += 1f; }

                if (caster.lastDetectedFirst.distance == 0f) continue;
                averagePosition += caster.lastFirstHitLocal;
                count += 1f;
            }

            if (count == 0f) return raycastingPosition;

            return averagePosition;
        }


        Vector3 CalculateTargetPosition()
        {
            Vector3 raycastingPosition = transform.position;
            Vector3 averagePosition = Vector3.zero;
            float count = 0f;

            for (int i = 0; i < spiderCasters.Length; i++)
            {
                var caster = spiderCasters[i];
                if (caster.anyHit == false) continue;

                if (caster.lastDetectedFirst.distance != 0f)
                {
                    averagePosition += caster.lastFirstHitLocal;
                    count += 1f;
                }

                //if ( caster.lastDetectedSec.distance != 0f)
                //{
                //    averagePosition += caster.lastSecHitLocal;
                //    count += 1f;
                //}
            }

            if (count == 0f) return raycastingPosition;

            averagePosition.x = 0f;
            averagePosition.z = 0f;
            averagePosition = transform.TransformPoint(averagePosition / count);

            averagePosition = Vector3.SmoothDamp(raycastingPosition, averagePosition, ref sd_position, 0.05f, 100000f, Time.deltaTime);

            return averagePosition;
        }

        Vector3 sd_position = Vector3.zero;

        void AnalyzeRaycast(RaycastHit hit, ref Vector3 average, SpiderRaycaster caster, bool firstHit)
        {
            if (hit.distance <= 0) return;
            average += hit.normal;
            AddRaycastCounterNormal(hit.normal);

            Vector3 local = firstHit ? caster.lastFirstHitLocal : caster.lastSecHitLocal;

            if (local.y > -GroundingCatchRange)
            {
                if (jumpCulldown > 0f)
                {
                    if (local.y > GroundingCatchRange * 2f) SetGrounded(true);
                }
                else SetGrounded(true);
            }

            if (local.y < lowestLocal.y) lowestLocal = local;
            else if (local.y > highestLocal.y) highestLocal = local;
        }

        void SetGrounded(bool gr)
        {
            isGrounded = gr;

            if (gr)
            {
                //isJumping = false;
            }

            if (Mecanim) Mecanim.SetBool("IsGrounded", gr);
            if (ToAssignHelperVars) ToAssignHelperVars.User_SetIsGrounded(gr);
        }

        public float GroundingCatchRange = 0.01f;

        #region Spidercast helper class

        class SpiderRaycaster
        {
            public RaycastHit firstHit;
            public RaycastHit secondaryHit;

            public RaycastHit lastDetectedFirst;
            public RaycastHit lastDetectedSec;

            public Vector3 lastFirstHitLocal;
            public Vector3 lastSecHitLocal;

            public bool anyHit;

            public void Reset()
            {
                firstHit = new RaycastHit();
                secondaryHit = new RaycastHit();
                lastDetectedFirst = new RaycastHit();
                lastDetectedSec = new RaycastHit();

                lastFirstHitLocal = Vector3.zero;
                lastSecHitLocal = Vector3.zero;

                //hits = 0;
                anyHit = false;
            }

            public void Cast(Vector3 rayStart, int step, DEMO_LegsAnim_SpiderMover mover)
            {
                anyHit = false;

                Vector3 p1, p2, helpDir, baseDir;

                baseDir = GetDirection(step, mover);
                helpDir = mover.raycastRotation * baseDir;
                baseDir.y = 0f;
                p1 = GetRaycastPoint00(rayStart, helpDir, mover.raycastRotation * baseDir, mover);
                p2 = GetRaycastPoint01(rayStart, helpDir, mover);

                if (Physics.Linecast(p1, p2, out firstHit, mover.GroundMask, QueryTriggerInteraction.Ignore))
                {
                    anyHit = true;
                    lastDetectedFirst = firstHit;
                    lastFirstHitLocal = mover.InversePoint(firstHit.point);
                }
                else // Not detected
                {
                    if (lastDetectedFirst.distance > 0f)
                    {
                        if (Vector3.Distance(mover.TransformPoint(lastFirstHitLocal), lastDetectedFirst.point) < mover.HitMemoryDistance)
                            firstHit = lastDetectedFirst;
                        else
                            lastDetectedFirst = new RaycastHit();
                    }
                }


                p1 = GetRaycastPoint10(rayStart, helpDir, mover);
                p2 = GetRaycastPoint11(step, p1, mover);
                if (Physics.Linecast(p1, p2, out secondaryHit, mover.GroundMask, QueryTriggerInteraction.Ignore))
                {
                    anyHit = true;
                    lastDetectedSec = secondaryHit;
                    lastSecHitLocal = mover.InversePoint(secondaryHit.point);
                }
                else // Not detected
                {
                    if (lastDetectedSec.distance > 0f)
                    {
                        if (Vector3.Distance(mover.TransformPoint(lastSecHitLocal), lastDetectedSec.point) < mover.HitMemoryDistance)
                            secondaryHit = lastDetectedSec;
                        else
                            lastDetectedSec = new RaycastHit();
                    }
                }

            }

            public static Vector3 GetDirection(int step, DEMO_LegsAnim_SpiderMover mover)
            {
                if (step == 0) return Vector3.down;
                else if (step == 1) return FEngineering.GetAngleDirectionYZ(-mover.FirstAngle);
                else if (step == 2) return FEngineering.GetAngleDirectionYZ(180 + mover.FirstAngle);
                else if (step == 3) return FEngineering.GetAngleDirectionYX(-mover.FirstAngle * mover.CollapseSides);
                else if (step == 4) return FEngineering.GetAngleDirectionYX(180 + mover.FirstAngle * mover.CollapseSides);
                return Vector3.zero;
            }

            public static Vector3 GetDirection2(int step, DEMO_LegsAnim_SpiderMover mover)
            {
                if (step == 0) return Vector3.down;
                else if (step == 1) return FEngineering.GetAngleDirectionYZ(270f - mover.SecondAngle);
                else if (step == 2) return FEngineering.GetAngleDirectionYZ(270f + mover.SecondAngle);
                else if (step == 3) return FEngineering.GetAngleDirectionYX(270f - mover.SecondAngle * Mathf.Lerp(0.5f, 1f, mover.CollapseSides));
                else if (step == 4) return FEngineering.GetAngleDirectionYX(270f + mover.SecondAngle * Mathf.Lerp(0.5f, 1f, mover.CollapseSides));
                return Vector3.zero;
            }

            public Vector3 GetRaycastDir1(int step, DEMO_LegsAnim_SpiderMover mover)
            {
                return mover.raycastRotation * GetDirection(step, mover);
            }

            public Vector3 GetRaycastPoint00(Vector3 origin, Vector3 dir1, Vector3 flatDir, DEMO_LegsAnim_SpiderMover mover)
            {
                return origin - flatDir * mover.CounterOffsets;
            }
            public Vector3 GetRaycastPoint01(Vector3 origin, Vector3 dir1, DEMO_LegsAnim_SpiderMover mover)
            {
                return origin + dir1 * mover.FirstRayDistance;
            }

            public Vector3 GetRaycastPoint10(Vector3 origin, Vector3 ray1Dir, DEMO_LegsAnim_SpiderMover mover)
            {
                return origin + ray1Dir * mover.FirstRayDistance * mover.SecondCastAlong;
            }

            public Vector3 GetRaycastPoint11(int step, Vector3 rayPoint10, DEMO_LegsAnim_SpiderMover mover)
            {
                return rayPoint10 + mover.raycastRotation * GetDirection2(step, mover) * mover.SecondRayDistance;
            }

        }


        SpiderRaycaster[] spiderCasters = new SpiderRaycaster[4];

        void ResetSpidercasters()
        {
            for (int i = 0; i < 4; i++)
            {
                if (spiderCasters[i] == null) spiderCasters[i] = new SpiderRaycaster();
                spiderCasters[i].Reset();
            }
        }

        void ProceedSpidercasts()
        {
            Vector3 rayStart = GetRaycastsOrigin();

            for (int i = 0; i < 4; i++)
            {
                spiderCasters[i].Cast(rayStart, i + 1, this);
            }
        }

        Vector3 lowestLocal;
        Vector3 highestLocal;

        #endregion


        Vector3 GetRaycastsOrigin()
        {
            return transform.position + raycastRotation * (Vector3.Scale(OriginOffset, transform.localScale));
        }


        #region Raycasts Processing Pool

        int currentNormalIDs = 0;
        List<RaycastCounter> raycastCounters = new List<RaycastCounter>();
        void ReloadRaycastCounter()
        {
            currentNormalIDs = 0;
        }

        void ReInitializeRaycastCounterPool(int count)
        {
            ReloadRaycastCounter();
            if (raycastCounters.Count == count) return;

            raycastCounters.Clear();
            for (int i = 0; i < count; i++)
            {
                raycastCounters.Add(new RaycastCounter());
            }
        }

        void AddRaycastCounterNormal(Vector3 normal)
        {
            for (int i = 0; i < currentNormalIDs; i++)
            {
                if (raycastCounters[i].normal == normal)
                {
                    raycastCounters[i].count += 1;
                    return;
                }
            }

            raycastCounters[currentNormalIDs].normal = normal;
            raycastCounters[currentNormalIDs].count = 1;
            currentNormalIDs += 1;
        }

        class RaycastCounter
        {
            public Vector3 normal;
            public int count = 0;
        }

        #endregion



        #region Editor Gizmos Code

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                if (spiderCasters[0] == null) ResetSpidercasters();
                raycastRotation = transform.rotation;
                ProceedSpidercasts();
            }

            Vector3 raysOrigin = GetRaycastsOrigin();

            Gizmos.color = Color.white * 0.7f;

            for (int i = 1; i <= 4; i += 1)
            {
                Vector3 rayDir1 = spiderCasters[0].GetRaycastDir1(i, this);

                Vector3 flatDir = SpiderRaycaster.GetDirection(i, this);
                flatDir.y = 0f;

                Vector3 ray1P1 = spiderCasters[0].GetRaycastPoint00(raysOrigin, rayDir1, raycastRotation * flatDir, this);
                Vector3 ray1P2 = spiderCasters[0].GetRaycastPoint01(raysOrigin, rayDir1, this);

                Vector3 ray2P1 = spiderCasters[0].GetRaycastPoint10(raysOrigin, rayDir1, this);
                Vector3 ray2P2 = spiderCasters[0].GetRaycastPoint11(i, ray2P1, this);

                Gizmos.DrawLine(ray1P1, ray1P2);
                Gizmos.DrawLine(ray2P1, ray2P2);
            }

            Gizmos.color = Color.yellow * 0.9f;
            Handles.color = Gizmos.color * 0.6f;

            for (int i = 0; i < 4; i += 1)
            {
                var caster = spiderCasters[i];

                if (caster.anyHit)
                {
                    if (caster.firstHit.distance != 0f) Gizmos.DrawRay(caster.firstHit.point, caster.firstHit.normal * 0.1f);
                    if (caster.secondaryHit.distance != 0f) Gizmos.DrawRay(caster.secondaryHit.point, caster.secondaryHit.normal * 0.1f);

                    if (i == 0)
                    {
                        if (caster.firstHit.distance != 0f)
                            Handles.CircleHandleCap(0, caster.firstHit.point, Quaternion.LookRotation(caster.firstHit.normal, -caster.firstHit.normal), HitMemoryDistance * 0.5f, EventType.Repaint);
                    }
                }

            }


            Vector3 pos = CalculateTargetPositionLocal();
            Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.05f, EventType.Repaint);
            Handles.DrawLine(pos, transform.position);
            Handles.SphereHandleCap(0, transform.position, Quaternion.identity, 0.02f, EventType.Repaint);

        }

#endif

        #endregion



        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor(typeof(DEMO_LegsAnim_SpiderMover))]
        public class DEMO_LegsAnim_SpiderMoverEditor : UnityEditor.Editor
        {
            public DEMO_LegsAnim_SpiderMover Get { get { if (_get == null) _get = (DEMO_LegsAnim_SpiderMover)target; return _get; } }
            private DEMO_LegsAnim_SpiderMover _get;

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                GUILayout.Space(4f);

                //for (int i = 0; i < 5; i++)
                //{
                //    EditorGUILayout.LabelField("[" + i + "] Dir = " + SphereCaster.GetDirection(i, Get.AngleSpread));
                //}

                //GUILayout.Space(4f);

                if (Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Raycast Pool Details", EditorStyles.boldLabel);

                    for (int i = 0; i < Get.currentNormalIDs; i++)
                    {
                        EditorGUILayout.LabelField("[" + i + "] " + Get.raycastCounters[i].normal + " :: " + Get.raycastCounters[i].count);
                    }
                }

                Get.perf.Editor_Display("Algorithm Duration:");
            }
        }

#endif

        #endregion


    }
}