using FIMSpace.Basics;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    public class DEMO_LegsAnim_SpiderMover2 : MonoBehaviour
    {
        public Fimp_JoystickInput JoystickInput;
        public Transform ToRotate;
        public Animator Mecanim;
        public bool StrafeMode = false;
        [Space(4)]
        public float MovementSpeed = 2f;
        public float RotateToSpeed = 2f;
        [Space(4)]
        public LayerMask GroundMask = 0 >> 1;
        [Space(4)]
        public float JumpPower = 3f;

        Vector2 moveDirectionLocal;
        //bool isGrounded = true;
        //float jumpRequest = 0f;

        [Range(0f, 360f)]
        public float UpAxisAngle = 0f;
        Quaternion raycastRotation = Quaternion.identity;


        void Start()
        {
            if (ToRotate == null) ToRotate = transform;
            raycastRotation = transform.rotation;
            UpAxisAngle = transform.eulerAngles.y;
            ResetSpherecasters();
        }


        void Update()
        {

            #region Input Update

            //if (Input.GetKeyDown(KeyCode.Space)) jumpRequest = JumpPower;

            moveDirectionLocal = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) moveDirectionLocal += Vector2.left;
            else if (Input.GetKey(KeyCode.D)) moveDirectionLocal += Vector2.right;

            if (Input.GetKey(KeyCode.W)) moveDirectionLocal += Vector2.up;
            else if (Input.GetKey(KeyCode.S)) moveDirectionLocal += Vector2.down;

            if (JoystickInput.OutputValue != Vector2.zero)
            {
                moveDirectionLocal.x += JoystickInput.OutputValue.x;
                moveDirectionLocal.y += JoystickInput.OutputValue.y;
            }

            #endregion


            #region Main Movement Control Apply

            Vector3 moveDirectionWorld = Vector3.zero;

            if (moveDirectionLocal != Vector2.zero)
            {
                moveDirectionLocal.Normalize();

                Quaternion flatCamRot = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
                moveDirectionWorld = flatCamRot * new Vector3(moveDirectionLocal.x, 0f, moveDirectionLocal.y);

                UpAxisAngle = Quaternion.LookRotation(moveDirectionWorld).eulerAngles.y;

                if (Mecanim) Mecanim.SetBool("Moving", true);
            }
            else
            {
                if (Mecanim) Mecanim.SetBool("Moving", false);
            }

            #endregion


            ProceedSpherecasts();
            ReInitializeRaycastCounterPool(sphereCasters.Length * sphereCasters[0].alloc.Length);

            Vector3 averageNormal = Vector3.zero;

            for (int s = 0; s < sphereCasters.Length; s++) // Check all spherecasters
            {
                if (sphereCasters[s].hits > 0)
                {
                    var hit = sphereCasters[s].closest;
                    averageNormal += hit.normal;
                    AddRaycastCounterNormal(hit.normal);
                }
            }


            if (averageNormal != Vector3.zero)
            {
                averageNormal.Normalize();

                Quaternion xzRotationRef = Quaternion.FromToRotation(Vector3.up, averageNormal);
                raycastRotation = xzRotationRef * Quaternion.AngleAxis(UpAxisAngle, Vector3.up);

                Vector3 rotationNormal = Vector3.zero;
                for (int i = 0; i < currentNormalIDs; i++)
                {
                    for (int c = 0; c < raycastCounters[i].count; c++)
                    {
                        rotationNormal += raycastCounters[i].normal / ((float)c + 1);
                    }
                }

                rotationNormal.Normalize();
                xzRotationRef = Quaternion.FromToRotation(Vector3.up, rotationNormal);
                ToRotate.rotation = xzRotationRef * Quaternion.AngleAxis(UpAxisAngle, Vector3.up);
            }




            //Vector3 averageNormalWeighted = Vector3.zero;
            //Vector3 averageNormalRaw = Vector3.zero;
            //float distRange = RayRadius * RayRadius;


            //for (int i = 0; i < sphereCasts; i++)
            //{
            //    var hit = sphereCastAlloc[i];
            //    float distToOrigin = Vector3.SqrMagnitude(hit.point - transform.position);
            //    averageNormalRaw += hit.normal;
            //    averageNormalWeighted += hit.normal * (1f - (Mathf.Min(1f, distToOrigin / distRange) * 0.75f)); // Closer to origin = higher importance
            //}

            //averageNormalRaw.Normalize();
            //averageNormalWeighted.Normalize();

            //if (averageNormalRaw != Vector3.zero)
            //{
            //    averageNormalWeighted.Normalize();

            //    Quaternion xzRotationRef = Quaternion.FromToRotation(Vector3.up, averageNormalRaw);
            //    raycastRotation = xzRotationRef * Quaternion.AngleAxis(UpAxisAngle, Vector3.up);

            //    xzRotationRef = Quaternion.FromToRotation(Vector3.up, averageNormalWeighted);
            //    ToRotate.rotation = xzRotationRef * Quaternion.AngleAxis(UpAxisAngle, Vector3.up);
            //}

        }


        #region Spherecaster helper class

        class SphereCaster
        {
            public RaycastHit[] alloc = new RaycastHit[8];
            public RaycastHit closest;
            public int hits = 0;

            public void Reset()
            {
                hits = 0;
            }

            public void Cast(Vector3 rayStart, int step, Quaternion castOrientation, float spread, float sidesSpread, float radius, float distance, LayerMask mask, float sideRangeBoost, float forwardsRangeBoost, float originOffsetPower)
            {
                Vector3 dir = SphereCaster.GetDirection(step, spread);
                dir.y = 0f;
                rayStart += castOrientation * (-dir.normalized * radius * originOffsetPower);

                Vector3 rayDir = GetDirection(step, spread, sidesSpread);
                if (step >= 1 && step <= 2) distance *= sideRangeBoost;
                else if (step >= 3 && step <= 4) distance *= forwardsRangeBoost;

                hits = Physics.SphereCastNonAlloc(rayStart, radius, castOrientation * rayDir, alloc, distance, mask, QueryTriggerInteraction.Ignore);

                float nearest = float.MaxValue;

                for (int i = 0; i < hits; i++)
                {
                    if (alloc[i].distance < nearest) { nearest = alloc[i].distance; closest = alloc[i]; }
                }
            }

            public static Vector3 GetDirection(int step, float spread, float sidesSpread = 1f)
            {
                if (step == 0) return Vector3.down;
                else if (step == 1) return FEngineering.GetAngleDirectionXY(180 + spread * sidesSpread);
                else if (step == 2) return FEngineering.GetAngleDirectionXY(180 - spread * sidesSpread);
                else if (step == 3) return FEngineering.GetAngleDirectionZY(180 + spread);
                else if (step == 4) return FEngineering.GetAngleDirectionZY(180 - spread);
                return Vector3.zero;
            }
        }



        SphereCaster[] sphereCasters = new SphereCaster[5];

        void ResetSpherecasters()
        {
            for (int i = 0; i < 5; i++)
            {
                if (sphereCasters[i] == null) sphereCasters[i] = new SphereCaster();
                sphereCasters[i].Reset();
            }
        }

        void ProceedSpherecasts()
        {
            Vector3 rayStart = GetRaycastsOrigin();
            float radius = RayRadius * transform.localScale.x;

            for (int i = 0; i < 5; i++)
            {
                sphereCasters[i].Cast(rayStart, i, raycastRotation, AngleSpread, SidesSpread, radius, RayDistance, GroundMask, SidesRangeBoost, ForwardsRangeBoost, OriginOffsetPower);
            }
        }

        #endregion


        [Header("Raycasting Setup")]
        public Vector3 OriginOffset = Vector3.up;
        [Range(0.01f, 0.5f)] public float RayRadius = 0.3f;
        [Range(0.05f, 1f)] public float RayDistance = 1f;
        [Range(0f, 45f)] public float AngleSpread = 30f;
        [Range(0.1f, 3f)] public float SidesSpread = 1f;
        [Range(1f, 2f)] public float SidesRangeBoost = 1f;
        [Range(1f, 2f)] public float ForwardsRangeBoost = 1f;
        [Range(-5f, 5f)] public float OriginOffsetPower = 1f;


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
                if (sphereCasters[0] == null) ResetSpherecasters();

                raycastRotation = transform.rotation;
                ProceedSpherecasts();
            }

            Vector3 rayStart = GetRaycastsOrigin();

            Gizmos.color = Color.yellow;

            for (int i = 0; i < sphereCasters.Length; i++)
            {
                if (sphereCasters[i].hits > 0) Gizmos.DrawRay(sphereCasters[i].closest.point, sphereCasters[i].closest.normal);
            }

            Gizmos.color = Color.white * 0.6f;
            GizmosDrawSpherecast(0,rayStart, raycastRotation * SphereCaster.GetDirection(0, AngleSpread));
            GizmosDrawSpherecast(1,rayStart, raycastRotation * SphereCaster.GetDirection(1, AngleSpread, SidesSpread), SidesRangeBoost);
            GizmosDrawSpherecast(2,rayStart, raycastRotation * SphereCaster.GetDirection(2, AngleSpread, SidesSpread), SidesRangeBoost);
            GizmosDrawSpherecast(3,rayStart, raycastRotation * SphereCaster.GetDirection(3, AngleSpread), ForwardsRangeBoost);
            GizmosDrawSpherecast(4,rayStart, raycastRotation * SphereCaster.GetDirection(4, AngleSpread), ForwardsRangeBoost);

            Gizmos.color *= 0.7f;

            for (int i = 0; i < sphereCasters.Length; i++)
            {
                for (int c = 0; c < sphereCasters[i].hits; c++)
                    Gizmos.DrawRay(sphereCasters[i].alloc[c].point, sphereCasters[i].alloc[c].normal);
            }

        }


        [Range(0, 360f)] public float Test = 0f;
        void GizmosDrawSpherecast(int step, Vector3 origin, Vector3 direction, float distanceBoost = 1f)
        {
            Vector3 dir = SphereCaster.GetDirection(step, AngleSpread);
            dir.y = 0f;
            origin += raycastRotation * (-dir.normalized * RayRadius * transform.localScale.x * OriginOffsetPower);

            Vector3 end = origin + direction * ((RayDistance * distanceBoost) - RayRadius / 2f);
            Gizmos.DrawWireSphere(origin, RayRadius);

            Quaternion rayRot = Quaternion.LookRotation(direction, -direction);
            Gizmos.DrawLine(origin + rayRot * Vector3.forward * RayRadius, end + rayRot * Vector3.forward * RayRadius);
            Gizmos.DrawLine(origin - rayRot * Vector3.forward * RayRadius, end - rayRot * Vector3.forward * RayRadius);
            Gizmos.DrawLine(origin + rayRot * Vector3.right * RayRadius, end + rayRot * Vector3.right * RayRadius);
            Gizmos.DrawLine(origin - rayRot * Vector3.right * RayRadius, end - rayRot * Vector3.right * RayRadius);

            Gizmos.DrawWireSphere(end, RayRadius);
        }


#endif
        #endregion



        #region Editor Class
#if UNITY_EDITOR
        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor(typeof(DEMO_LegsAnim_SpiderMover2))]
        public class DEMO_LegsAnim_SpiderMover2Editor : UnityEditor.Editor
        {
            public DEMO_LegsAnim_SpiderMover2 Get { get { if (_get == null) _get = (DEMO_LegsAnim_SpiderMover2)target; return _get; } }
            private DEMO_LegsAnim_SpiderMover2 _get;

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                GUILayout.Space(4f);

                for (int i = 0; i < 5; i++)
                {
                    EditorGUILayout.LabelField("[" + i + "] Dir = " + SphereCaster.GetDirection(i, Get.AngleSpread));
                }

                GUILayout.Space(4f);

                if (Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Raycast Pool Details", EditorStyles.boldLabel);

                    //for (int i = 0; i < Get.sphereCasters.Length; i++)
                    //{
                    //    for (int j = 0; j < Get.sphereCasters[i].hits; j += 1)
                    //        EditorGUILayout.LabelField("[" + i + "] " + Get.sphereCasters[i].alloc[j].normal + " :: " + Get.sphereCasters[i].alloc[j].distance);
                    //}
                }
            }
        }
#endif
        #endregion


    }
}