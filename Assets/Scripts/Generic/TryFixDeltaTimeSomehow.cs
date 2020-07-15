using UnityEngine;

/*

https://forum.unity.com/threads/time-deltatime-not-constant-vsync-camerafollow-and-jitter.430339/page-2
https://medium.com/@alen.ladavac/the-elusive-frame-timing-168f899aec92


I THINK I'VE GOT SOMETHING:

	OPENGL (-force-glcore)
	vSyncCount = 1
	targetFrameRate = -1
	captureFrameRate = RefreshRate

Vsync still has effect in OpenGL. Then 60 fps (or the refresh rate) are assumed and
LookAt camera works nicely with TimeMode = RefreshRate (or deltaTime = Time.deltaTime / Time.timeScale).

In DirectX, CaptureFrameRate seems to void vsync.

Time.unscaledDeltaTime does not receive a correction when captureFrameRate is applied.

*/


public class TryFixDeltaTimeSomehow : MonoBehaviour
	{
	[Range(0,2)]
	public int vSyncCount = 1;

	[Range(-1,200)]
	public int targetFrameRate = -1;

	public enum CaptureRefreshRateMode { Explicit, RefreshRate, SmoothDeltaTime };
	public CaptureRefreshRateMode captureRefreshRateMode = CaptureRefreshRateMode.Explicit;
	[Range(0,200)]
	public int captureFramerate = 0;


	void Update ()
		{
		QualitySettings.vSyncCount = vSyncCount;
		Application.targetFrameRate = targetFrameRate;

		switch (captureRefreshRateMode)
			{
			case CaptureRefreshRateMode.Explicit:
				Time.captureFramerate = captureFramerate;
				break;

			case CaptureRefreshRateMode.RefreshRate:
				Time.captureFramerate = Screen.currentResolution.refreshRate;
				break;

			case CaptureRefreshRateMode.SmoothDeltaTime:
				Time.captureFramerate = Mathf.RoundToInt(1.0f / Time.smoothDeltaTime);
				break;
			}
		}
	}


