using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Abiogenesis3d
{
    public enum GraphicsFormatSubset
    {
        R8G8B8A8_UNorm = GraphicsFormat.R8G8B8A8_UNorm,
        R8G8B8A8_SRGB = GraphicsFormat.R8G8B8A8_SRGB,
    }

    [Serializable]
    public class UPixelatorCameraInfo
    {
        public Camera cam;

        [Header("Subpixel")]
        public bool snap = true;
        public bool stabilize = true;

        // [Header("Parallax")]
        // public float positionSpeed;
        // public float rotationSpeed;

        [HideInInspector] public Renderer renderQuadRenderer;
        [HideInInspector] public RenderTexture renderTexture;
        [HideInInspector] public Transform renderQuad;
        [HideInInspector] public UPixelatorRenderHandler renderHandler;
    }

    [ExecuteInEditMode]
    [DefaultExecutionOrder(999)]
    public class UPixelator : MonoBehaviour
    {
        [Range(1, 5)]
        public int pixelMultiplier = 3;

        [Range(2, 16)]
        public int ditherRepeatSize = 8;

        // public Vector2Int testExtraPixels;
        Vector2Int extraPixels;

        [HideInInspector] public Vector2Int screenSize;
        [HideInInspector] public Vector2Int renderTextureSize;

        // TODO: solve for gamma color space
        public GraphicsFormatSubset graphicsFormat = GraphicsFormatSubset.R8G8B8A8_UNorm;
        LayerMask layerMask = 1 << 5; // UI

        float camSliceGap = 0.05f;

        // NOTE: this is wip, need to multiply ui positions with this
        [Range(0.1f, 1)]
        float uPixelatorZoom = 1;
        // TODO: UPixelatorZoomOffset to zoom at mouse position

        // NOTE: this is the main camera that renders all UPixelator textures
        [HideInInspector] public Camera uPixelatorCam;

        [Header("To ignore a camera add UPixelatorCameraIgnore component to it.")]
        public bool autoDetectCameras = true;
        public List<UPixelatorCameraInfo> cameraInfos = new List<UPixelatorCameraInfo>();
        public Material renderQuadTransparentMat;
        public Material renderQuadOpaqueMat;

        float lastHandleInits;
        float handleInitsEvery = 1;

        [HideInInspector] public bool isSnappablesDirty;

        void OnValidate()
        {
            if (ditherRepeatSize % 2 != 0) ditherRepeatSize += 1;
            // if (testExtraPixels.x % 2 != 0) testExtraPixels.x += 1;
            // if (testExtraPixels.y % 2 != 0) testExtraPixels.y += 1;
        }

        void CheckForInstances()
        {
            var existingInstances = FindObjectsOfType<UPixelator>();
            if (existingInstances.Length > 1)
            {
                Debug.Log($"UPixelator: There should only be one active instance in the scene. Deactivating: {name}");
                enabled = false;
                return;
            }
        }

        void OnEnable()
        {
            lastHandleInits = 0;
            if (uPixelatorCam) uPixelatorCam.enabled = true;
            CheckForInstances();
        }

        void OnDisable()
        {
            if (uPixelatorCam) uPixelatorCam.enabled = false;
            foreach (var camInfo in cameraInfos)
                if (camInfo.renderHandler) camInfo.renderHandler.enabled = false;
        }

        void LateUpdate()
        {
            HandleIgnoreComponents();
            HandleGetCameras();
            HandleUPixelatorCam();
            screenSize = GetScreenSize();
            renderTextureSize = GetRenderTextureSize();
            CleanupLeftoverHandlers();

            transform.position = Vector3.one * 1000;

            foreach (var camInfo in cameraInfos)
            {
                if (!camInfo.cam) continue;

                camInfo.cam.allowMSAA = false;

                // NOTE: if orthographicSize is 0 some divisions will break
                if (camInfo.cam.orthographicSize == 0)
                {
                    Debug.LogWarning("UPixelator: Cameras should not have orthographicSize 0, setting to 1.");
                    camInfo.cam.orthographicSize = 1;
                }

                UpdateCamInfo(camInfo);

                // NOTE: update renderQuad.localPosition here in LateUpdate because
                // render callbacks are called after transforms were already updated this frame
                // camInfo.renderHandler.UpdateRenderQuadPosition();
            }

            HandleInits();

            if (isSnappablesDirty)
                HandleSnappables();
        }

        void CleanupLeftoverHandlers()
        {
            var handlers = gameObject.GetComponentsInChildren<UPixelatorRenderHandler>(true);
            foreach (var handler in handlers)
            {
                // need to remove handler gameobjects that are not in cameraInfos
                if (cameraInfos.All(c => c.renderHandler != handler))
                    DestroyImmediate(handler.gameObject);
            }
        }

        public Camera GetFirstCamera()
        {
            return cameraInfos
                .Where(c => c.cam)
                .Where(c => c.cam.gameObject.activeInHierarchy)
                .FirstOrDefault()?.cam;
        }

        Type GetIgnoredType()
        {
            return typeof(UPixelatorCameraIgnore);
        }

        void HandleGetCameras()
        {
            if (!autoDetectCameras) return;

            var cameras = FindObjectsOfType<Camera>().ToList();
            var camerasToRemove = cameras.Where(c => c.GetComponent(GetIgnoredType()) != null).ToArray();

            foreach(var cam in camerasToRemove)
            {
                var match = cameraInfos.FirstOrDefault(c => c.cam == cam);
                if (match != default) cameraInfos.Remove(match);
            }
            cameras.RemoveAll(c => camerasToRemove.Contains(c));

            foreach (var camInfo in cameraInfos)
            {
                if (!cameras.Contains(camInfo.cam))
                {
                    if (camInfo.renderHandler != null)
                        camInfo.renderHandler.gameObject?.SetActive(false);
                }
            }

            foreach (var cam in cameras)
            {
                var camInfo = cameraInfos.FirstOrDefault(c => c.cam == cam);
                if (camInfo != null)
                {
                    if (camInfo.renderHandler != null)
                        camInfo.renderHandler.gameObject.SetActive(true);
                    continue;
                }
                else
                {
                    camInfo = new UPixelatorCameraInfo {cam = cam};
                    cameraInfos.Add(camInfo);
                }
            }

            cameraInfos = cameraInfos.OrderBy(c => GetCamDepthOr0(c.cam)).ToList();

            var depthsNotUnique = cameraInfos
                .Where(c => c.cam?.gameObject.activeInHierarchy ?? false)
                .GroupBy(c => GetCamDepthOr0(c.cam))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key).ToArray();

            if (depthsNotUnique.Length > 0)
            {
                Debug.LogWarning("UPixelator: Please make cameras have unique depth.");
                return;
            }
        }

        // Vector2Int GetScreenSize()
        // {
        //     var s = new Vector2Int(Screen.width, Screen.height);
        //     if (s.x % 2 != 0) s.x -= 1;
        //     if (s.y % 2 != 0) s.y -= 1;
        //     return s;

        //     // TODO: how to reproduce Screen being the scene view instead of game view?
        //     //       in that case use cam.pixelHeight, cam.pixelWidth
        //     var oddFix = Vector2Int.zero;
        //     var widthModulo = Screen.width % pixelMultiplier;
        //     var heightModulo = Screen.height % pixelMultiplier;
        //     if (widthModulo != 0) oddFix.x = pixelMultiplier - widthModulo;
        //     if (heightModulo != 0) oddFix.y = pixelMultiplier - heightModulo;

        //     int width = (Screen.width + oddFix.x) / pixelMultiplier * pixelMultiplier;
        //     int height = (Screen.height + oddFix.y) / pixelMultiplier * pixelMultiplier;

        //     return new Vector2Int(width, height);
        // }

        Vector2Int GetScreenSize()
        {
            // TODO: how to reproduce Screen being the scene view instead of game view?
            //       in that case use cam.pixelHeight, cam.pixelWidth
            return new Vector2Int(Screen.width, Screen.height);
        }

        Vector2Int GetRenderTextureSize()
        {
            // return GetScreenSize();

            Vector2Int padding = pixelMultiplier > 1 ? GetRenderTexturePadding() : Vector2Int.zero;
            Vector2Int size = screenSize / pixelMultiplier + padding;

            // NOTE: ensures that Unity does not permanently break the camera component...
            if (size.x < 1) size.x = 1;
            if (size.y < 1) size.y = 1;

            if (size.x > screenSize.x) size.x = screenSize.x;
            if (size.y > screenSize.y) size.y = screenSize.y;

            return size;
        }

        // NOTE: must always have even numbers, which dither is setup to have
        // NOTE: lowest padding should always be 2 to hide subpixel offset correction on screen edges
        public Vector2Int GetRenderTexturePadding()
        {
            // return Vector2Int.zero;
            return Vector2Int.one * ditherRepeatSize + extraPixels * 2; // + testExtraPixels * 2;
        }

        // TODO: cleanup unreferenced quads
        void EnsureRenderQuad(UPixelatorCameraInfo camInfo)
        {
            if (camInfo.renderQuad) return;
            camInfo.renderQuad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            DestroyImmediate(camInfo.renderQuad.GetComponent<MeshCollider>());

            var parent = new GameObject().transform;
            parent.transform.SetParent(transform, false);

            camInfo.renderQuad.SetParent(parent, false);
        }

        GraphicsFormat GetGraphicsFormatFromSubset(GraphicsFormatSubset s)
        {
            return (GraphicsFormat)(int) s;
        }

        void EnsureRenderTexture(UPixelatorCameraInfo camInfo)
        {
            if (camInfo.renderTexture) return;

            // NOTE: this is initial resolution, do changes from HandleResizeTexture
            camInfo.renderTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 16, GetGraphicsFormatFromSubset(graphicsFormat));
            camInfo.renderTexture.name = "RenderTexture - " + camInfo.cam.name;
            camInfo.renderTexture.filterMode = FilterMode.Point;
            camInfo.renderTexture.Create();
        }

        void EnsureRenderQuadRenderer(UPixelatorCameraInfo camInfo)
        {
            if (camInfo.renderQuadRenderer) return;
            camInfo.renderQuadRenderer = camInfo.renderQuad.GetComponent<Renderer>();
        }

        void HandleRenderQuadMaterial(UPixelatorCameraInfo camInfo)
        {
            EnsureRenderQuadRenderer(camInfo);

            if (!renderQuadTransparentMat)
            {
                Debug.LogWarning("renderQuadTransparentMat needs to be assigned");
                return;
            }
            if (!renderQuadOpaqueMat)
            {
                Debug.LogWarning("renderQuadOpaqueMat needs to be assigned");
                return;
            }

            var index = 0;
            foreach (UPixelatorCameraInfo c in cameraInfos)
            {
                if (c.cam == null) continue;
                if (!c.cam.gameObject.activeInHierarchy) continue;
                if (c == camInfo) break;
                index++;
            }
            var mat = index == 0 ? renderQuadOpaqueMat : renderQuadTransparentMat;

            var r = camInfo.renderQuadRenderer;
            if (r.sharedMaterial.shader == mat.shader) return;

            r.sharedMaterial = Instantiate(mat);
            // TODO: why this isn't working
            // #if UNITY_2021_1_OR_NEWER && !UNITY_2022_1_OR_NEWER
            #if UNITY_2021_1_OR_NEWER
            #if !UNITY_2022_1_OR_NEWER
            r.sharedMaterial.renderQueue = 3000;
            #endif
            #endif
            r.sharedMaterial.mainTexture = camInfo.renderTexture;

            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.allowOcclusionWhenDynamic = false;
        }

        // public Vector2 extraCamRect;
        // Rect GetCamRect(UPixelatorCameraInfo camInfo)
        // {
        //     var s = GetScreenSize();
        //     float rectWidth = s.x / camInfo.renderTexture.width;
        //     float rectHeight = s.y / camInfo.renderTexture.height;

        //     return new Rect(0, 0, rectWidth + extraCamRect.x, rectHeight + extraCamRect.y);
        // }

        void HandleResizeTexture(UPixelatorCameraInfo camInfo)
        {
            EnsureRenderTexture(camInfo);

            if (renderTextureSize.x != camInfo.renderTexture.width ||
                renderTextureSize.y != camInfo.renderTexture.height ||
                GetGraphicsFormatFromSubset(graphicsFormat) != camInfo.renderTexture.graphicsFormat)
            {
                camInfo.cam.targetTexture = null;
                camInfo.renderTexture.Release();
                camInfo.renderTexture.graphicsFormat = GetGraphicsFormatFromSubset(graphicsFormat);
                camInfo.renderTexture.width = renderTextureSize.x;
                camInfo.renderTexture.height = renderTextureSize.y;
                camInfo.cam.targetTexture = camInfo.renderTexture;
            }
        }

        float GetCamDepthOr0(Camera cam)
        {
            if (cam != null) return cam.depth;
            return 0;
        }

        void HandleRenderQuad(UPixelatorCameraInfo camInfo)
        {
            EnsureRenderQuad(camInfo);

            camInfo.renderQuad.gameObject.layer = (int) Math.Log(layerMask, 2);
            camInfo.renderQuad.parent.gameObject.layer = (int) Math.Log(layerMask, 2);

            camInfo.renderQuad.name = "RenderQuad - " + camInfo.cam.name;
            camInfo.renderQuad.parent.name = "UPixelator - " + camInfo.cam.name;

            // var screenRatio = (float)screenSize.x / screenSize.y;
            // camInfo.renderQuad.localScale = new Vector2(screenRatio, 1) * 2;
            camInfo.renderQuad.localScale = new Vector2(renderTextureSize.x, renderTextureSize.y) / screenSize.y * 2 * pixelMultiplier;

            // sort by depth
            var pos = camInfo.renderQuad.localPosition;
            var camInfos = cameraInfos.Where(x => x.cam).ToList();
            pos.z = (camInfos.Count -1 - camInfos.IndexOf(camInfo)) * camSliceGap;
            camInfo.renderQuad.localPosition = pos;
        }

        void EnsureUPixelatorRenderHandler(UPixelatorCameraInfo camInfo)
        {
            if (camInfo.renderHandler) return;

            camInfo.renderHandler = camInfo.renderQuad.parent.GetComponent<UPixelatorRenderHandler>();
            if (!camInfo.renderHandler) camInfo.renderHandler = camInfo.renderQuad.parent.gameObject.AddComponent<UPixelatorRenderHandler>();
        }

        void HandleUPixelatorRenderHandler(UPixelatorCameraInfo camInfo)
        {
            EnsureUPixelatorRenderHandler(camInfo);

            camInfo.renderHandler.uPixelator = this;
            camInfo.renderHandler.camInfo = camInfo;
            camInfo.renderHandler.enabled = camInfo.cam.enabled;
        }

        void UpdateCamInfo(UPixelatorCameraInfo camInfo)
        {
            // CalculateParallax(camInfo);

            HandleRenderQuad(camInfo);
            HandleResizeTexture(camInfo);
            HandleRenderQuadMaterial(camInfo);
            HandleUPixelatorRenderHandler(camInfo);
        }

        void EnsureUPixelatorCam()
        {
            if (uPixelatorCam) return;
            uPixelatorCam = GetComponent<Camera>();
            if (!uPixelatorCam) uPixelatorCam = gameObject.AddComponent<Camera>();
        }

        void HandleUPixelatorCam()
        {
            EnsureUPixelatorCam();

            // TODO: -0.2 needed for TMPro Text to be shown
            // TODO: but at least -0.17 is needed for pixelMultiplier==1 to not clip?
            uPixelatorCam.nearClipPlane = - 0.2f;
            // TODO: why extra distance is needed?
            var farClipPlaneExtraDistance = 1;
            uPixelatorCam.farClipPlane = cameraInfos.Count * camSliceGap + farClipPlaneExtraDistance;

            // uPixelatorCam.clearFlags = CameraClearFlags.SolidColor;
            // uPixelatorCam.backgroundColor = Color.black;

            uPixelatorCam.orthographic = true;
            uPixelatorCam.orthographicSize = uPixelatorZoom;

            uPixelatorCam.allowHDR = false;
            uPixelatorCam.allowMSAA = false;

            uPixelatorCam.cullingMask |= layerMask;

            // NOTE: this needs to be after all previous uPixelator textures to render properly
            if (cameraInfos.Count > 0)
                uPixelatorCam.depth = cameraInfos.Max(c => GetCamDepthOr0(c.cam)) + 1;
        }

        bool BelongsToCamera(Vector3 position, Camera camera)
        {
            float offset = 0.1f;

            Vector3 viewportPoint = camera.WorldToViewportPoint(position);
            Rect extendedViewport = new Rect(-offset, -offset, 1 + 2 * offset, 1 + 2 * offset);
            return (
                extendedViewport.Contains(viewportPoint) && // isInViewport
                viewportPoint.z >= 0 && viewportPoint.z <= camera.farClipPlane // isWithinDepth
            );
        }

        void HandleInits()
        {
            // TODO: randomize this to not create processing spikes
            if (Time.time - lastHandleInits > handleInitsEvery)
            {
                lastHandleInits = Time.time;
                isSnappablesDirty = true;
            }
        }

        void HandleSnappables()
        {
            var snappables = FillSnappablesList();

            foreach (var camInfo in cameraInfos)
            {
                if (!camInfo.cam) continue;
                if (!camInfo.renderHandler) continue;

                camInfo.renderHandler.snappables.Clear();
                camInfo.renderHandler.snappables.Add(camInfo.renderHandler.GetCamSnappable());
            }

            foreach (var s in snappables)
            {
                if (s.isCamera) continue;
                var observerCam = cameraInfos.Find(c => BelongsToCamera(s.transform.position, c.cam));
                observerCam?.renderHandler?.snappables?.Add(s);
            }

            isSnappablesDirty = false;
        }

        // TODO: move to helpers
        void EnsureRequiredComponent(Type type)
        {
            if (!gameObject.GetComponent(type)) gameObject.AddComponent(type);
        }

        void HandleIgnoreComponents()
        {
            EnsureRequiredComponent(typeof(MultiCameraEvents));
            EnsureRequiredComponent(typeof(MultiCameraEventsIgnore));
            EnsureRequiredComponent(typeof(UPixelatorCameraIgnore));
            EnsureRequiredComponent(typeof(PixelArtEdgeHighlightsIgnore));
        }

        List<UPixelatorSnappable> FillSnappablesList()
        {
            var snappableArray = GameObject.FindObjectsOfType<UPixelatorSnappable>();

            foreach (var snappable in snappableArray)
                snappable.nested = new List<UPixelatorSnappable>();

            var roots = new List<UPixelatorSnappable>();
            foreach (var snappable in snappableArray)
            {
                UPixelatorSnappable parent = snappable.transform.parent?.GetComponentInParent<UPixelatorSnappable>();
                if (parent == null) roots.Add(snappable);
                else parent.nested.Add(snappable);
            }

            var flattened = new List<UPixelatorSnappable>();
            Action<UPixelatorSnappable> flatten = null;
            flatten = s => {
                flattened.Add(s);
                s.nested.ForEach(c => flatten(c));
            };
            roots.ForEach(flatten);

            return flattened;
        }
    }
}
