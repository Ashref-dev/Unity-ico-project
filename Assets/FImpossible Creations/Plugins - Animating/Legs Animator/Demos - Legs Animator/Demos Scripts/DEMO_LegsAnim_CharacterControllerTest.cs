using FIMSpace.Basics;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    public class DEMO_LegsAnim_CharacterControllerTest : MonoBehaviour
    {
        public CharacterController Controller;
        public FDebug_PerformanceTest performanceTest = new FDebug_PerformanceTest();

        void Update()
        {
            performanceTest.Start(gameObject);
            Controller.Move(Vector3.forward * Time.deltaTime);
            performanceTest.Finish(gameObject);
        }


        #region Editor Class
#if UNITY_EDITOR
        /// <summary>
        /// FM: Editor class component to enchance controll over component from inspector window
        /// </summary>
        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor(typeof(DEMO_LegsAnim_CharacterControllerTest))]
        public class DEMO_LegsAnim_CharacterControllerTestEditor : UnityEditor.Editor
        {
            public DEMO_LegsAnim_CharacterControllerTest Get { get { if (_get == null) _get = (DEMO_LegsAnim_CharacterControllerTest)target; return _get; } }
            private DEMO_LegsAnim_CharacterControllerTest _get;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                GUILayout.Space(4f);
                DrawPropertiesExcluding(serializedObject, "m_Script");

                serializedObject.ApplyModifiedProperties();

                Get.performanceTest.Editor_DisplayAlways("Elapsed:");
            }
        }
#endif
        #endregion


    }
}