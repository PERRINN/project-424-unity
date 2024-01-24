
﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class VIOSOCamera : MonoBehaviour
{
    // We have several VIOSOCamera components around, but there's only one eye point.
    // DynismaInputDevice pass the values here directly as they come.

    public static Vector3 eyePointPos;
    public static Vector3 eyePointRot;

    // Test mode for manually specifying the eye point values

    public bool testMode = false;
    public Vector3 testEyePointPos;
    public Vector3 testEyePointRot;

    // Debug

    public static Matrix4x4 lastMatrix;
    public static FrustumPlanes lastFrustum;

    // VIOSO API

    public enum ERROR
    {
        NONE = 0,         /// No error, we succeeded
        GENERIC = -1,     /// a generic error, this might be anything, check log file
        PARAMETER = -2,   /// a parameter error, provided parameter are missing or inappropriate
        INI_LOAD = -3,    /// ini could notbe loaded
        BLEND = -4,       /// blend invalid or coud not be loaded to graphic hardware, check log file
        WARP = -5,        /// warp invalid or could not be loaded to graphic hardware, check log file
        SHADER = -6,      /// shader program failed to load, usually because of not supported hardware, check log file
        VWF_LOAD = -7,    /// mappings file broken or version mismatch
        VWF_FILE_NOT_FOUND = -8, /// cannot find mapping file
        NOT_IMPLEMENTED = -9,     /// Not implemented, this function is yet to come
        NETWORK = -10,        /// Network could not be initialized
        FALSE = -16,        /// No error, but nothing has been done
    };

    [DllImport("VIOSO_Plugin64")]
    private static extern IntPtr GetRenderEventFunc();
    [DllImport("VIOSO_Plugin64")]
    private static extern ERROR Init(ref int id, string name );
    [DllImport("VIOSO_Plugin64")]
    private static extern ERROR UpdateTex(int id, IntPtr texHandleSrc, IntPtr texHandleDest );
    [DllImport("VIOSO_Plugin64")]
    private static extern ERROR Destroy(int id);
    [DllImport("VIOSO_Plugin64")]
    private static extern ERROR GetError(int id, ref int err);
    [DllImport("VIOSO_Plugin64")]
    private static extern ERROR GetViewProj(int id, ref Vector3 pos, ref Vector3 rot, ref Matrix4x4 view, ref Matrix4x4 proj);
    [DllImport("VIOSO_Plugin64")]
    private static extern ERROR GetViewClip(int id, ref Vector3 pos, ref Vector3 rot, ref Matrix4x4 view, ref FrustumPlanes clip);
    [DllImport("VIOSO_Plugin64")]
    private static extern void SetTimeFromUnity(float t);

    private Camera cam;
    private int viosoID = -1;
    private Quaternion orig_rot = Quaternion.identity;
    private Vector3 orig_pos = Vector3.zero;
    private Dictionary<RenderTexture, IntPtr> texMap = new Dictionary<RenderTexture, IntPtr>();
    private bool startCalled = false;
    private bool loopError = false;

    public static void ScreenShot(string path)
    {
        Texture2D tex;
        int w;
        int h;
        TextureFormat fmt;
        if (null != RenderTexture.active)
        {
            w = RenderTexture.active.width;
            h = RenderTexture.active.height;
            fmt = TextureFormat.RGB24;
        }
        else
        {
            w = Screen.width;
            h = Screen.height;
            fmt = TextureFormat.RGB24;
        }
        tex = new Texture2D(w, h, fmt, false);

        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);

        byte[] io = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, io);
    }


    private void OnEnable()
    {
        startCalled = false;
        loopError = false;
    }


    private void Start()
    {
        startCalled = true;
        cam = GetComponent<Camera>();

        orig_rot = cam.transform.localRotation;
        orig_pos = cam.transform.localPosition;

        ERROR err = ERROR.FALSE;
        err = Init(ref viosoID, cam.name);
        if (err == ERROR.NONE)
        {
            GL.IssuePluginEvent(GetRenderEventFunc(), viosoID); // this will initialize warper in Unity Graphic Library context
            int err1 = 0;
            GetError(viosoID, ref err1);
            err = (ERROR)err1;
            if (err != ERROR.NONE)
            {
                Debug.Log($"VIOSO Initialization of warper failed [{err}]. See VIOSO_Plugin64.log for details.");
            }
        }
        else
        {
            Debug.Log($"VIOSO Initialization attempt of warper failed with error [{err}]. See VIOSO_Plugin64.log for details.");
        }

        if (err != ERROR.NONE)
        {
            Debug.Log($"VIOSO Failed to init camera [{err}]. See VIOSO_Plugin64.log for details.");
        }
    }


    private void Update()
    {
        // Start is not called when disabling and re-enabling the component, nor when reloading the scripts in runtime.
        if (!startCalled)
        {
            Start();
        }
    }

    private void OnDisable()
    {
        if (viosoID != -1)
        {
            ERROR err = Destroy(viosoID);
            if (err != ERROR.NONE)
            {
                Debug.Log($"VIOSO Error [{err}] de-initializing. See VIOSO_Plugin64.log for details.");
                if (err == ERROR.PARAMETER)
                {
                    Debug.LogWarning("VIOSO If picture was missing check that VIOSOWarpBlend.ini and VWF files are present.");
                }
            }

            viosoID = -1;
            cam.transform.localRotation = orig_rot;
            cam.transform.localPosition = orig_pos;
            cam.ResetProjectionMatrix();
        }
    }

    private void OnPreCull()
    {
        if (viosoID != -1)
        {
            if (testMode)
            {
                eyePointPos = testEyePointPos;
                eyePointRot = testEyePointRot;
            }

            Matrix4x4 mV = Matrix4x4.identity;
            FrustumPlanes pl = new FrustumPlanes();
            ERROR err = GetViewClip(viosoID, ref eyePointPos, ref eyePointRot, ref mV, ref pl);
            if (err == ERROR.NONE)
            {
                mV = mV.transpose;
                Quaternion q = mV.rotation;
                Vector3 p = mV.GetColumn(3);
                cam.transform.localRotation = orig_rot * q;
                cam.transform.localPosition = orig_pos + p;

                // TODO: Camera shifting possible here? Must coordinate with UI (ViosoEyePointUICorrection)
                cam.projectionMatrix = Matrix4x4.Frustum(pl);

                lastMatrix = mV;
                lastFrustum = pl;
            }
            else
            {
                if (!loopError)
                {
                    Debug.Log($"VIOSO Error [{err}] getting view clip. Check that VIOSOWarpBlend.ini and VWF files are present. See VIOSO_Plugin64.log for details.");
                    loopError = true;
                }
            }
        }
    }

    private void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        RenderTexture.active = destination;
        if (viosoID != -1)
        {
            IntPtr dst;
            if (!texMap.TryGetValue(source, out dst))
            {
                dst = source.GetNativeTexturePtr();
                texMap[source] = dst;
            }
            ERROR err = UpdateTex(viosoID, dst, IntPtr.Zero);
            if (err != ERROR.NONE && !loopError)
            {
                Debug.Log($"VIOSO Error [{err}] updating texture. See VIOSO_Plugin64.log for details.");
                loopError = true;
            }
            SetTimeFromUnity(Time.timeSinceLevelLoad);
            GL.IssuePluginEvent(GetRenderEventFunc(), viosoID);
        }
    }

 }
