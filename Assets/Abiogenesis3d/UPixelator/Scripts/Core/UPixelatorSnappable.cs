using System.Collections.Generic;
using UnityEngine;

namespace Abiogenesis3d
{
    [ExecuteInEditMode]
    public class UPixelatorSnappable : MonoBehaviour
    {
        [HideInInspector] public bool isCamera;
        // NOTE: this snaps to the grid defined by the pixel size
        public bool snapPosition = true;
        public bool snapLocalScale;
        [Range(0, 1)] public float snapScaleValue = 0.05f;

        public bool snapRotation;
        public bool isLocalRotation;
        [Range(0, 8)] public int divisions360 = 5;
        public Vector3 snapRotationAngles;

        Vector3 storedPosition;
        Quaternion storedRotation;
        Vector3 storedLocalScale;

        // NOTE: needed because exiting play mode resets values before onEndCameraRendering calls reset so it resets to default 0 position
        bool storePositionDirty;
        bool storeRotationDirty;
        bool storeLocalScaleDirty;

        // NOTE: default to true because snapping should be accompanied with restoring otherwise it can lead to issues
        bool log = true;

        [HideInInspector] public List<UPixelatorSnappable> nested = new List<UPixelatorSnappable>();

        void Update()
        {
            // TODO: get/set or other
            if (divisions360 > 0)
            {
                snapRotationAngles = Vector3.one * 360 / Mathf.Pow(2, divisions360);
            }
        }

        void OnEnable()
        {
            var uPixelator = FindObjectOfType<UPixelator>();
            if (uPixelator) uPixelator.isSnappablesDirty = true;
        }

        void Log(string str)
        {
            if (!log) return;
            Debug.Log(str);
        }

        public void StorePosition()
        {
            if (storePositionDirty) Log("Already stored position, waiting for restore, " + name);
            else
            {
                storedPosition = transform.position;
                storePositionDirty = true;
            }
        }
        public void RestorePosition()
        {
            // if (permanent) {} else
            if (storePositionDirty) transform.position = storedPosition;
            else Log("No stored position, " + name);
            storePositionDirty = false;
        }

        public void StoreRotation()
        {
            if (storeRotationDirty) Log("Already stored rotation, waiting for restore, " + name);
            else
            {
                if (isLocalRotation) storedRotation = transform.localRotation;
                else storedRotation = transform.rotation;
                storeRotationDirty = true;
            }
        }
        public void RestoreRotation()
        {
            if (storeRotationDirty) {
                if (isLocalRotation) transform.localRotation = storedRotation;
                else transform.rotation = storedRotation;
            }
            else Log("No stored rotation, " + name);
            storeRotationDirty = false;
        }


        public void StoreLocalScale()
        {
            if (storeLocalScaleDirty) Log("Already stored localScale, waiting for restore, " + name);
            else
            {
                storedLocalScale = transform.localScale;
                storeLocalScaleDirty = true;
            }
        }
        public void RestoreLocalScale()
        {
            if (storeLocalScaleDirty) transform.localScale = storedLocalScale;
            else Log("No stored localScale, " + name);
            storeLocalScaleDirty = false;
        }

        public void SnapLocalRotation(Vector3 snapAngles)
        {
            Vector3 localEulerAngles = transform.localEulerAngles;
            localEulerAngles.x = Snap(transform.localEulerAngles.x, snapAngles.x);
            localEulerAngles.y = Snap(transform.localEulerAngles.y, snapAngles.y);
            localEulerAngles.z = Snap(transform.localEulerAngles.z, snapAngles.z);
            transform.localEulerAngles = localEulerAngles;
        }

        public void SnapRotation(Vector3 snapAngles)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.x = Snap(transform.eulerAngles.x, snapAngles.x);
            eulerAngles.y = Snap(transform.eulerAngles.y, snapAngles.y);
            eulerAngles.z = Snap(transform.eulerAngles.z, snapAngles.z);
            transform.eulerAngles = eulerAngles;
        }

        public void SnapLocalScale(float snapValue)
        {
            transform.localScale = Snap(transform.localScale, snapValue);
        }

        public static Vector3 Snap(Vector3 vector, float divisor)
        {
            vector.x = Snap(vector.x, divisor);
            vector.y = Snap(vector.y, divisor);
            vector.z = Snap(vector.z, divisor);
            return vector;
        }

        public static float Snap(float number, float divisor)
        {
            if (divisor == 0) return number;
            return Mathf.Round(number / divisor) * divisor;
        }

        public Vector3 SnapPosition(Quaternion camRotation, float snapSize, Vector3 initialPosition)
        {
            // TODO: mark all snap functions as static, add Transform target
            Transform target = transform;

            Vector3 unrotatedPosition = Quaternion.Inverse(camRotation) * (target.position - initialPosition);
            Vector3 snappedPosition = Snap(unrotatedPosition, snapSize);
            // NOTE: don't snap z, makes the shadows flicker less
            snappedPosition.z = unrotatedPosition.z;

            Vector3 snapDiff = unrotatedPosition - snappedPosition;
            Vector3 rotatedVector = camRotation * snappedPosition + initialPosition;
            target.position = rotatedVector;

            return snapDiff;
        }

        public static Vector3 RawSnapPosition(Vector3 position, Quaternion camRotation, float snapSize)
        {
            Vector3 unrotatedPosition = Quaternion.Inverse(camRotation) * position;
            return camRotation * Snap(unrotatedPosition, snapSize);
        }
    }
}
