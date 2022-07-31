using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

public class VIOSOCamera : MonoBehaviour
{

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

    //private const string dllLoc = "GfxPlugin_VIOSO64";// @"D:\Unity\1st\Assets\Plugins\GfxPlugin_VIOSO64.dll";
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


    private void Start()
    {
        cam = GetComponent<Camera>();

        orig_rot = cam.transform.localRotation;
        orig_pos = cam.transform.localPosition;

        ERROR err = ERROR.FALSE;
        err = Init(ref viosoID, cam.name );
        if (ERROR.NONE == err)
        {
            GL.IssuePluginEvent(GetRenderEventFunc(), viosoID); // this will initialize warper in Unity Graphic Library context
            int err1 = 0;
            GetError(viosoID, ref err1);
            err = (ERROR)err1;
            if (ERROR.NONE != err)
            {
                Debug.Log("Initialization of warper failed.");
            }
        }
        else
        {
            Debug.Log(string.Format("Initialization attempt of warper failed with eror %i.", err ) );
        }

        if (ERROR.NONE != err)
        {
            Debug.Log("Failed to init camera.");
        }
    }

    private void OnDisable()
    {
        if (-1 != viosoID)
        {
            ERROR err = Destroy(viosoID);
            viosoID = -1;
        }
    }

    private void OnPreCull()
    {
        if (-1 != viosoID)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            Vector3 rot = new Vector3(0, 0, 0);
            Matrix4x4 mV = Matrix4x4.identity;

            Matrix4x4 mP = new Matrix4x4();
            FrustumPlanes pl = new FrustumPlanes();
            if (ERROR.NONE == GetViewClip(viosoID, ref pos, ref rot, ref mV, ref pl))
            {
                mV = mV.transpose;
                Quaternion q = mV.rotation;
                Vector3 p = mV.GetColumn(3);
                cam.transform.localRotation = orig_rot * q;
                cam.transform.localPosition = orig_pos + p;

                mP = Matrix4x4.Frustum(pl);
                cam.projectionMatrix = mP;
            }
        }
    }

    private void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        RenderTexture.active = destination;
        if (-1 != viosoID)
        {
            IntPtr dst;
            if (!texMap.TryGetValue(source, out dst))
            {
                dst = source.GetNativeTexturePtr();
                texMap[source] = dst;
            }
            UpdateTex(viosoID, dst, IntPtr.Zero);
            SetTimeFromUnity(Time.timeSinceLevelLoad);
            GL.IssuePluginEvent(GetRenderEventFunc(), viosoID);
        }
    }

 }
