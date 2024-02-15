using UnityEngine;

namespace Abiogenesis3d
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(999)]
    public class FollowTransformUI : MonoBehaviour
    {
        // NOTE: hidden until fixed
        [HideInInspector]
        public RenderMode renderMode = RenderMode.ScreenSpaceOverlay;

        UPixelator uPixelator;

        FitCanvasToScreen fitCanvasToScreen;

        public Canvas parentCanvas;
        public Transform parentTransform;
        public RectTransform rectTransform;

        void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        Camera GetCam()
        {
            // return uPixelator.uPixelatorCam; // why is this not working?
            // return Camera.main;
            return uPixelator.GetFirstCamera() ?? Camera.main;
        }

        void HandleParent()
        {
            if (!uPixelator)
            {
                uPixelator = GameObject.FindObjectOfType<UPixelator>();
                if (!uPixelator) return;
            }

            // NOTE: do this every time because parent can change
            parentCanvas = transform.parent.GetComponent<Canvas>();
            if (!parentCanvas)
            {
                GameObject parentCanvasGO = new GameObject("ParentCanvas");
                parentCanvasGO.transform.SetParent(transform.parent);
                parentCanvas = parentCanvasGO.AddComponent<Canvas>();
                transform.SetParent(parentCanvasGO.transform);
            }
            // NOTE: do this every time because parent can change
            // TODO: refactor this to track the parent and clean up previous parent components
            fitCanvasToScreen = parentCanvas.GetComponent<FitCanvasToScreen>();
            if (!fitCanvasToScreen)
                fitCanvasToScreen = parentCanvas.gameObject.AddComponent<FitCanvasToScreen>();

            if (renderMode == RenderMode.WorldSpace)
            {
                Debug.LogWarning("WorldSpace is currently not supported. Switching to Overlay.");
                renderMode = RenderMode.ScreenSpaceOverlay;
            }
            if (renderMode == RenderMode.ScreenSpaceCamera)
            {
                Debug.LogWarning("ScreenSpaceCamera is currently not supported. Switching to Overlay.");
                renderMode = RenderMode.ScreenSpaceOverlay;
            }
            parentCanvas.renderMode = renderMode;
            // parentCanvas.worldCamera = GetCam();
            parentCanvas.planeDistance = -0.1f;
            parentCanvas.pixelPerfect = true;

            string renderModeStr = "";
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) renderModeStr = "Overlay";
            else if (parentCanvas.renderMode == RenderMode.WorldSpace) renderModeStr = "World";
            else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera) renderModeStr = "Camera";

            fitCanvasToScreen.gameObject.layer = transform.gameObject.layer;
            fitCanvasToScreen.name = name + " - Parent: " + renderModeStr;
            fitCanvasToScreen.cam = GetCam();

            fitCanvasToScreen.Init();

            // get first non canvas parent to follow
            parentTransform = parentCanvas.transform.parent;
            while (parentTransform && parentTransform.GetComponent<Canvas>() != null)
                parentTransform = parentTransform.parent;
        }

        void LateUpdate()
        {
            HandleParent();
            if (!uPixelator) return;

            var cam = GetCam();
            if (cam == null) return;

            var camInfo = uPixelator.cameraInfos.Find(x => x.cam == cam);
            if (camInfo == null) return;

            fitCanvasToScreen.DoUpdate();

            Vector3 viewportPoint = cam.WorldToViewportPoint(parentTransform.position);

            if (float.IsNaN(viewportPoint.x) ||
                float.IsNaN(viewportPoint.y) ||
                float.IsNaN(viewportPoint.z))
            {
                // TODO: this happens when scripts are recompiled or undo is pressed..
                // TODO: why, probably camera is destroyed?
                // Debug.Log("viewportPoint: " + viewportPoint + ", parentTransform.position: " + parentTransform.position);
            }
            else {
                rectTransform.anchoredPosition = default;
                // rectTransform.SetPositionAndRotation(viewportPoint, Quaternion.Euler( 0, 0, 0));
                rectTransform.anchorMin = viewportPoint;
                rectTransform.anchorMax = viewportPoint;
            }
        }
    }
}
