//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

#if false

namespace VehiclePhysics
{

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DIJOYSTATE2
	{
	public int    lX;                     /* x-axis position              */
	public int    lY;                     /* y-axis position              */
	public int    lZ;                     /* z-axis position              */
	public int    lRx;                    /* x-axis rotation              */
	public int    lRy;                    /* y-axis rotation              */
	public int    lRz;                    /* z-axis rotation              */

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public int[]  rglSlider;              /* 2 extra axes positions       */

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	public int[] rgdwPOV;                 /* 4 POV directions             */

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
	public byte[] rgbButtons;             /* 128 buttons                  */

	public int    lVX;                    /* x-axis velocity              */
	public int    lVY;                    /* y-axis velocity              */
	public int    lVZ;                    /* z-axis velocity              */
	public int    lVRx;                   /* x-axis angular velocity      */
	public int    lVRy;                   /* y-axis angular velocity      */
	public int    lVRz;                   /* z-axis angular velocity      */

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public int[]  rglVSlider;             /* 2 extra axes velocities      */

	public int    lAX;                    /* x-axis acceleration          */
	public int    lAY;                    /* y-axis acceleration          */
	public int    lAZ;                    /* z-axis acceleration          */
	public int    lARx;                   /* x-axis angular acceleration  */
	public int    lARy;                   /* y-axis angular acceleration  */
	public int    lARz;                   /* z-axis angular acceleration  */

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public int[]  rglASlider;             /* 2 extra axes accelerations   */

	public int    lFX;                    /* x-axis force                 */
	public int    lFY;                    /* y-axis force                 */
	public int    lFZ;                    /* z-axis force                 */
	public int    lFRx;                   /* x-axis torque                */
	public int    lFRy;                   /* y-axis torque                */
	public int    lFRz;                   /* z-axis torque                */
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public int[]  rglFSlider;             /* 2 extra axes forces          */

	public bool CompareTo (DIJOYSTATE2 other)
		{
		return lX == other.lX
			&& lY == other.lY
			&& lZ == other.lZ
			&& lRx == other.lRx
			&& lRy == other.lRy
			&& lRz == other.lRz
			&& ObjectUtility.ArraysEqual(rglSlider, other.rglSlider)
			&& ObjectUtility.ArraysEqual(rgdwPOV, other.rgdwPOV)
			&& ObjectUtility.ArraysEqual(rgbButtons, other.rgbButtons)

			&& lVX == other.lVX
			&& lVY == other.lVY
			&& lVZ == other.lVZ
			&& lVRx == other.lVRx
			&& lVRy == other.lVRy
			&& lVRz == other.lVRz
			&& ObjectUtility.ArraysEqual(rglVSlider, other.rglVSlider)

			&& lAX == other.lAX
			&& lAY == other.lAY
			&& lAZ == other.lAZ
			&& lARx == other.lARx
			&& lARy == other.lARy
			&& lARz == other.lARz
			&& ObjectUtility.ArraysEqual(rglASlider, other.rglASlider)

			&& lFX == other.lFX
			&& lFY == other.lFY
			&& lFZ == other.lFZ
			&& lFRx == other.lFRx
			&& lFRy == other.lFRy
			&& lFRz == other.lFRz
			&& ObjectUtility.ArraysEqual(rglFSlider, other.rglFSlider);
		}
	}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DIDEVICEINSTANCE
	{
	public UInt32 dwSize;
	public Guid guidInstance;
	public Guid guidProduct;
	public UInt32 dwDevType;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	public String instanceName;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	public String productName;
	public Guid guidFFDriver;
	public UInt16 wUsagePage;
	public UInt16 wUsage;
	}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DIDEVICEOBJECTINSTANCE
	{
	UInt32 dwSize;
	Guid guidType;
	UInt32 dwOfs; // NOTE: This does NOT correspond to the directinput axis offset, it is the offset from the driver
	UInt32 dwType;
	UInt32 dwFlags;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	public String name;
	UInt32 dwFFMaxForce;
	UInt32 dwFFForceResolution;
	UInt16 wCollectionNumber;
	UInt16 wDesignatorIndex;
	UInt16 wUsagePage;
	UInt16 wUsage;
	UInt32 dwDimension;
	UInt16 wExponent;
	UInt16 wReportId;
	}


public static class DIWrapper
	{
	const string DllName = "DIWrapper";

	// Do not call Initialize or Release here directly.
	// Use DIWrapperManager.Initialize() and DIWrapperManager.Release() instead.
	//
	// Calling Initialize here disposes any devices that may be already in use.
	// DIWrapperManager allows multiple components using the wrapper simulatenously.
	// Also handles exceptions (i.e. DLL not found).

	[DllImport(DllName)] public static extern bool Initialize();
	[DllImport(DllName)] public static extern void Release();

	// Device enumeration and generic info

	[DllImport(DllName)] public static extern int GetNumDevices();
	[DllImport(DllName)] public static extern bool GetDeviceInfo(int n, out DIDEVICEINSTANCE device);

	// Device selection, initialization and acquisition

	[DllImport(DllName)] public static extern bool SelectDevice(int index);
	[DllImport(DllName)] public static extern bool OpenDevice();
	[DllImport(DllName)] public static extern void CloseDevice();

	[DllImport(DllName)] public static extern bool AcquireExclusive();
	[DllImport(DllName)] public static extern void UnacquireExclusive();

	// Device information

	[DllImport(DllName)] public static extern int GetNumButtons();
	[DllImport(DllName)] public static extern int GetNumAxis();
	[DllImport(DllName)] public static extern int GetNumFFBAxis();

	[DllImport(DllName)] public static extern bool GetButtonInfo(int n, out DIDEVICEOBJECTINSTANCE button);
	[DllImport(DllName)] public static extern bool GetAxisInfo(int n, out DIDEVICEOBJECTINSTANCE axis);
	[DllImport(DllName)] public static extern bool GetFFAxisInfo(int n, out DIDEVICEOBJECTINSTANCE ffaxis);

	// Force feedback

	[DllImport(DllName)] public static extern bool SelectFFBAxis(int n);
	[DllImport(DllName)] public static extern bool PlayConstant(int magnitude);
	[DllImport(DllName)] public static extern bool StopConstant();
	[DllImport(DllName)] public static extern bool PlaySpring(int offset, int saturation, int coefficient);
	[DllImport(DllName)] public static extern bool StopSpring();
	[DllImport(DllName)] public static extern bool PlayDamper(int coefficient);
	[DllImport(DllName)] public static extern bool StopDamper();
	[DllImport(DllName)] public static extern bool PlayFriction(int saturation, int coefficient);
	[DllImport(DllName)] public static extern bool StopFriction();

	// Device state

	[DllImport(DllName)] public static extern bool Poll();
	[DllImport(DllName)] public static extern bool GetState(out DIJOYSTATE2 js);

	// Information & debug

	[DllImport(DllName)] public static extern int GetLastResult();
	[DllImport(DllName)] public static extern System.IntPtr GetLog();
	[DllImport(DllName)] public static extern void ClearLog();
	}


public static class DIWrapperManager
	{
	private static int s_referenceCount = 0;


	public static bool initialized { get { return s_referenceCount > 0; } }


	public static bool Initialize ()
		{
		if (s_referenceCount == 0)
			{
			// DLL not yet initialized. Initialize.
			// The DLL is loaded in the first call to a DIWrapper method, so if there's any issue
			// it will raise an exception here.

			try {
				#if UNITY_EDITOR
				if (UnityEditor.EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.StandaloneWindows
					&& UnityEditor.EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.StandaloneWindows64)
					{
					throw new Exception("Non-Windows build target. DirectInput works on Windows Standalone only.");
					}
				#endif

				if (DIWrapper.Initialize())
					{
					s_referenceCount = 1;
					return true;
					}
				}
			catch (Exception e)
				{
				UnityEngine.Debug.LogWarning("DIWrapperManager: DIWrapper.dll not found or couldn't be loaded.\nError: " + e.Message);
				}

			return false;
			}
		else
			{
			// Already initialized. Add a reference.

			s_referenceCount++;
			return true;
			}
		}


	public static void Release ()
		{
		if (s_referenceCount == 0)
			{
			// No pending initializations. Harmless exit.

			return;
			}
		else
		if (s_referenceCount == 1)
			{
			// Last reference. Release the DLL.

			DIWrapper.Release();
			s_referenceCount = 0;
			}
		else
			{
			// More than one initializations. Remove a reference.

			s_referenceCount--;
			}
		}
	}
}
#endif
