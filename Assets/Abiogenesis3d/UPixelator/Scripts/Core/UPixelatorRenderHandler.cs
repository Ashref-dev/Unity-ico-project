using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_PIPELINE_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace Abiogenesis3d
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(1000)]
    public class UPixelatorRenderHandler : MonoBehaviour
    {
        [HideInInspector] public UPixelator uPixelator;
        // NOTE: these are set by UPixelator after attaching this script
        public UPixelatorCameraInfo camInfo;

        UPixelatorSnappable camSnappable;

        public List<UPixelatorSnappable> snappables = new List<UPixelatorSnappable>();
        public Dictionary<UPixelatorSnappable, Vector3> initialPositions = new Dictionary<UPixelatorSnappable, Vector3>();
        Quaternion storedCamRotation;

        float origOrthoSize;

        enum SnapState {None, Unsnapped, Snapped};
        SnapState snapState = SnapState.None;

        public void LateUpdate()
        {
            if (camInfo.cam == null) return;

            EnsureCamSnappable();

            // NOTE: this ensures OnEndCameraRendering is called after other callbacks
            // otherwise the camera snapped position might be reset back too early

            #if UNITY_PIPELINE_URP
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            #else

            Camera.onPostRender -= PostRender;
            Camera.onPostRender += PostRender;

            #endif

            // NOTE: setting targetTexture must be done here instead of the render callbacks
            //  otherwise it will not be set the same frame
            HandleTargetTexture();

            // NOTE: for some reason when entering play mode LateUpdate is called twice
            //  so skip first snap and let it be called from OnPostRender
            if (snapState != SnapState.None) HandleSnap();
        }

        void HandleTargetTexture()
        {
            camInfo.cam.targetTexture = camInfo.renderTexture;
            Utils.RunAtEndOfFrameOrdered(() => {
                // NOTE: this is needed or else the Screen size is set to renderTexture size
                //  and events like Input.mousePosition that return pixel values are wrong
                // NOTE: this also prevents other cameras to affect the texture
                camInfo.cam.targetTexture = null;
            }, 0, this);
        }

        float GetSnapSize()
        {
            return uPixelator.pixelMultiplier * (2 * camInfo.cam.orthographicSize) / uPixelator.screenSize.y;
        }

        void EnsureCamSnappable()
        {
            if (camSnappable)
            {
            #if UNITY_PIPELINE_URP
                var camData = camInfo.cam?.GetComponent<UniversalAdditionalCameraData>();
                if (camData?.renderType == CameraRenderType.Overlay)
                    camSnappable.enabled = false;
            #endif
                return;
            }
            camSnappable = camInfo.cam.GetComponent<UPixelatorSnappable>();
            if (!camSnappable) camSnappable = camInfo.cam.gameObject.AddComponent<UPixelatorSnappable>();
            camSnappable.isCamera = true;
        }

        void HandleSnap()
        {
            if (!ShouldSnap()) return;
            if (snapState == SnapState.Snapped) return;

            snapState = SnapState.Snapped;

            Utils.RunAtEndOfFrameOrdered(() => {
                HandleUnsnap();
            }, 0, this);

            // NOTE: first store all or transform is changed by afterwards snapped parent
            foreach (UPixelatorSnappable snappable in snappables)
            {
                if (snappable == null) continue;
                if (!snappable.isActiveAndEnabled) continue;

                if (snappable.snapPosition) snappable.StorePosition();
                if (snappable.snapRotation) snappable.StoreRotation();
                if (snappable.snapLocalScale) snappable.StoreLocalScale();
            }

            var isCamRotationDirty = false;
            if (camSnappable != null)
            {
                if (camInfo.cam.transform.rotation != storedCamRotation)
                {
                    isCamRotationDirty = true;
                    storedCamRotation = camInfo.cam.transform.rotation;
                }
            }

            foreach (UPixelatorSnappable snappable in snappables)
            {
                if (snappable == null) continue;
                if (!snappable.isActiveAndEnabled) continue;
                if (snappable == camSnappable)
                {
                    UpdateRenderQuadPosition(HandleCamSnap());
                    continue;
                }

                if (isCamRotationDirty)
                    initialPositions[snappable] = snappable.transform.position;

                Vector3 initialPos = default;
                initialPositions.TryGetValue(snappable, out initialPos);
                if (snappable.snapPosition) snappable.SnapPosition(camInfo.cam.transform.rotation, GetSnapSize(), initialPos);

                if (snappable.snapRotation && !snappable.isLocalRotation) snappable.SnapRotation(snappable.snapRotationAngles);
                if (snappable.snapRotation && snappable.isLocalRotation) snappable.SnapLocalRotation(snappable.snapRotationAngles);
                if (snappable.snapRotation) snappable.SnapRotation(snappable.snapRotationAngles);
                if (snappable.snapLocalScale) snappable.SnapLocalScale(snappable.snapScaleValue);
            }
        }

        public bool ShouldSnap()
        {
            return camInfo.snap && camInfo.cam.orthographic;
        }

        public void UpdateRenderQuadPosition()
        {
            if (!ShouldSnap()) return;
            if (camSnappable == null) return;
            if (!camSnappable.isActiveAndEnabled) return;
            if (!camSnappable.snapPosition) return;

            camSnappable.StorePosition();
            UpdateRenderQuadPosition(HandleCamSnap());
            camSnappable.RestorePosition();
        }

        Vector3 HandleCamSnap()
        {
            var repeatSize = camInfo.stabilize ? uPixelator.ditherRepeatSize : 1;
            float camSnapSize = repeatSize * GetSnapSize();
            return camSnappable.SnapPosition(camInfo.cam.transform.rotation, camSnapSize, default);
        }

        void UpdateRenderQuadPosition(Vector3 camSnapDiff)
        {
            if (!camInfo.stabilize) return;

            // if (camSnapDiff == default) return;
            Vector3 localPosition = -camSnapDiff / camInfo.cam.orthographicSize;
            // NOTE: keep z, it is handled by the UPixelator based on depth
            localPosition.z = camInfo.renderQuad.localPosition.z;
            camInfo.renderQuad.localPosition = localPosition;

            // if (camInfo.stabilize && camInfo.cam.orthographic) {}
            // else renderQuad.localPosition = Vector3.zero;
        }

        void HandleUnsnap()
        {
            if (!ShouldSnap()) return;
            if (snapState == SnapState.Unsnapped) return;

            snapState = SnapState.Unsnapped;

            foreach (UPixelatorSnappable snappable in snappables)
            {
                if (snappable == null) continue;
                if (!snappable.isActiveAndEnabled) continue;

                if (snappable.snapPosition) snappable.RestorePosition();
                if (snappable.snapRotation) snappable.RestoreRotation();
                if (snappable.snapLocalScale) snappable.RestoreLocalScale();
            }
        }

        void OnEnable()
        {
            // NOTE: needed here to immediately resume rendering to texture instead of waiting next LateUpdate
            if (camInfo?.cam != null) camInfo.cam.targetTexture = camInfo.renderTexture;

        #if UNITY_PIPELINE_URP
            Utils.AddCallbackToStart<Action<ScriptableRenderContext, Camera>>(typeof(RenderPipelineManager), "beginCameraRendering", new Action<ScriptableRenderContext, Camera>(OnBeginCameraRendering));
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        #else
            Utils.AddCallbackToStart<Camera.CameraCallback>(typeof(Camera), "onPreRender", new Camera.CameraCallback(PreRender));
            Camera.onPostRender -= PostRender;
            Camera.onPostRender += PostRender;
        #endif
        }

        public UPixelatorSnappable GetCamSnappable()
        {
            if (!camSnappable) EnsureCamSnappable();
            return camSnappable;
        }

        void OnDisable()
        {
            if (camInfo?.cam != null) camInfo.cam.targetTexture = null;

            camSnappable = null;

        #if UNITY_PIPELINE_URP
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        #else
            Camera.onPreRender -= PreRender;
            Camera.onPostRender -= PostRender;
        #endif
        }

    #if UNITY_PIPELINE_URP
        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            PreRender(camera);
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            PostRender(camera);
        }
    #endif

        float GetVerticalOrthoSizeCorrection(float origSize)
        {
            float correction = origSize / Screen.height;

            float extraVerticalPixels = uPixelator.GetRenderTexturePadding().y * uPixelator.pixelMultiplier;
            // NOTE: ensures that Unity does not permanently break the camera component...
            if (extraVerticalPixels / 2 > Screen.height) extraVerticalPixels = Screen.height;

            return origSize + (extraVerticalPixels * correction);
        }

        void PreRender(Camera camera)
        {
            if (camera != camInfo.cam) return;

            HandleSnap();

        #if UNITY_PIPELINE_URP
        #else
            Rect pixelRect = camInfo.cam.pixelRect;
        #endif

            if (camInfo.renderTexture.width < 1 || camInfo.renderTexture.height < 1)
            {
                Debug.LogError("RenderTexture's width and height must be greater than 0");
                return;
            }

            origOrthoSize = camInfo.cam.orthographicSize;
            float newOrthoSize = GetVerticalOrthoSizeCorrection(origOrthoSize);
            // NOTE: ensures that Unity does not permanently break the camera component...
            if (newOrthoSize < 0.5) newOrthoSize = 0.5f;
            camInfo.cam.orthographicSize = newOrthoSize;

        #if UNITY_PIPELINE_URP
        #else
            camInfo.cam.pixelRect = pixelRect;
        #endif
        }

        void PostRender(Camera camera)
        {
            if (camera != camInfo.cam) return;

            // NOTE: cannot unsnap here of the urp overlay cameras will have wrong position
            // HandleUnsnap();

            // NOTE: this fixes blur that sometimes happens on certain texture resolutions
            //  probably because the leftover cam viewport rect size is not integers
            camInfo.cam.rect = new Rect(0, 0, 1, 1);
            camInfo.cam.orthographicSize = origOrthoSize;
        }
    }
}
