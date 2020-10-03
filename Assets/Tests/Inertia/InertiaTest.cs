

using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;


[RequireComponent(typeof(Rigidbody))]
public class InertiaTest : MonoBehaviour
	{
	public Inertia.Settings inertia = new Inertia.Settings();

	[Header("Force Application")]
	public Vector3 forcePosition = Vector3.right;
	public Vector3 forceVector = Vector3.forward;
	public float forceStart = 1.0f;
	public float forceDuration = 1.0f;

	[Header("Conditions")]
	public float deltaTime = 0.005f;
	public float timeScale = 0.01f;

	[Header("Display")]
	public Vector2 position = new Vector2(8, 8);
	public Font font;
	[Range(6,50)]
	public int fontSize = 16;
	[Range(6,50)]
	public int smallFontSize = 10;
	public Color fontColor = Color.white;


	// Private members


	Rigidbody m_rigidbody;
	Inertia m_inertiaHelper = new Inertia();
	Vector3 m_lastAngularVelocity;
	Vector3 m_lastEulerAngles;
	Vector3 m_lastEulerVelocity;
	Vector3 m_minEulerAngles;
	Vector3 m_maxEulerAngles;

	float m_internalTime;
	int m_internalFrame;

	string m_text;
	string m_results;
	GUIStyle m_textStyle = new GUIStyle();
	GUIStyle m_smallTextStyle = new GUIStyle();
	float m_boxWidth;
	float m_boxHeight;


	// Component methods


	void OnEnable ()
		{
		m_rigidbody = GetComponent<Rigidbody>();

		m_inertiaHelper.settings = inertia;
		m_inertiaHelper.Apply(m_rigidbody);
		m_lastAngularVelocity = m_rigidbody.angularVelocity;
		m_lastEulerAngles = m_rigidbody.rotation.eulerAngles;
		m_lastEulerVelocity = Vector3.zero;
		m_minEulerAngles = m_lastEulerAngles;
		m_maxEulerAngles = m_lastEulerAngles;

		Time.timeScale = timeScale;
		Time.fixedDeltaTime = deltaTime;
		m_internalTime = 0.0f;
		m_internalFrame = 0;

		m_text = "";
		m_results = "";
		}


	void Update ()
		{
		m_textStyle.font = font;
		m_textStyle.fontSize = fontSize;
		m_textStyle.normal.textColor = fontColor;

		m_smallTextStyle.font = font;
		m_smallTextStyle.fontSize = smallFontSize;
		m_smallTextStyle.normal.textColor = fontColor;

		m_inertiaHelper.DoUpdate(m_rigidbody);
		DebugUtility.DrawCrossMark(m_rigidbody.worldCenterOfMass, m_rigidbody.transform, GColor.accentPurple, 0.2f);

		Time.timeScale = timeScale;
		}


	void FixedUpdate ()
		{
		// Apply the force during the specified time

		if (m_internalTime >= forceStart && m_internalTime < forceStart + forceDuration)
			{
			ApplyForce();
			}

		// Retrieve results

		Vector3 angularAcceleration = (m_rigidbody.angularVelocity - m_lastAngularVelocity) / deltaTime;
		m_lastAngularVelocity = m_rigidbody.angularVelocity;

		Vector3 eulerAngles = m_rigidbody.rotation.eulerAngles;
		Vector3 eulerVelocity = DeltaAngle(m_lastEulerAngles, eulerAngles) / deltaTime;
		Vector3 eulerAcceleration = (eulerVelocity - m_lastEulerVelocity) / deltaTime;
		m_lastEulerAngles = eulerAngles;
		m_lastEulerVelocity = eulerVelocity;

		eulerVelocity *= Mathf.Deg2Rad;
		eulerAcceleration *= Mathf.Deg2Rad;

		eulerAngles = DeltaAngle(Vector3.zero, eulerAngles);
		m_minEulerAngles = Vector3.Min(m_minEulerAngles, eulerAngles);
		m_maxEulerAngles = Vector3.Max(m_maxEulerAngles, eulerAngles);

		m_text = $"#{m_internalFrame,-6} {m_internalTime,7:N3}s          X        Y        Z\n\nAngular Velocity:     {FormatVector(m_lastAngularVelocity, 5)}\nAngular Acceleration: {FormatVector(angularAcceleration, 5)}\n";

		m_text += $"\nEuler Angles Min:     {FormatVector(m_minEulerAngles,3,3)}\n             Max:     {FormatVector(m_maxEulerAngles,3,3)}\n";
		m_text += $"\nInertia Tensor:       {FormatVector(m_rigidbody.inertiaTensor,2,4)}\n      Rotation:       {FormatVector(DeltaAngle(Vector3.zero,m_rigidbody.inertiaTensorRotation.eulerAngles),3,3)}\n";

		m_text += $"\nEuler Angles:         {FormatVector(eulerAngles,3,3)}\nEuler Velocity:       {FormatVector(eulerVelocity,5)}\nEuler Acceleration:   {FormatVector(eulerAcceleration,5)}\n";

		if (m_internalTime > forceStart / 2 && m_internalTime < forceStart * 2 + forceDuration)
			m_results += $"#{m_internalFrame,-3} {m_internalTime,5:0.000} {angularAcceleration.x,10:0.000000} {angularAcceleration.y,10:0.000000} {angularAcceleration.z,10:0.000000}\n";

		// Advance physics time

		m_internalTime += deltaTime;
		m_internalTime = MathUtility.RoundDecimals(m_internalTime, 3);
		m_internalFrame++;
		}


	void ApplyForce ()
		{
		Vector3 applicationPoint = m_rigidbody.transform.TransformPoint(forcePosition);

		DebugUtility.DrawCrossMark(applicationPoint, GColor.solidRed, 0.2f);
		Debug.DrawLine(applicationPoint, applicationPoint + forceVector / 1000);

	 	m_rigidbody.AddForceAtPosition(forceVector, applicationPoint);
		}


	void OnGUI ()
		{
		// Compute box size

		Vector2 contentSize = m_textStyle.CalcSize(new GUIContent(m_text));
		float margin = m_textStyle.lineHeight * 1.2f;
		float headerHeight = GUI.skin.box.lineHeight;

		m_boxWidth = contentSize.x + margin;
		m_boxHeight = contentSize.y + headerHeight + margin / 2;

		// Compute box position

		float xPos = position.x < 0? Screen.width + position.x - m_boxWidth : position.x;
		float yPos = position.y < 0? Screen.height + position.y - m_boxHeight : position.y;

		// Draw telemetry box

		GUI.Box(new Rect(xPos, yPos, m_boxWidth, m_boxHeight), "Inertia Results");
		GUI.Label(new Rect(xPos + margin / 2, yPos + margin / 2 + headerHeight, Screen.width, Screen.height), m_text, m_textStyle);

		GUI.Label(new Rect(xPos, yPos + m_boxHeight + margin / 2, Screen.width, Screen.height), m_results, m_smallTextStyle);
		}


	string FormatVector (Vector3 v, int decimals = 1, int integers = 1)
		{
		// string[] formats = { ":N0", ":N1", "0.00", "0.000", "0.0000", "0.00000" };

		decimals = Mathf.Clamp(decimals, 0, 5);
		string format = $"{decimals+integers+2}:N{decimals}";
		return string.Format($"{{0,{format}}} {{1,{format}}} {{2,{format}}}", v.x, v.y, v.z);
		}


	Vector3 DeltaAngle (Vector3 fromAngles, Vector3 toAngles)
		{
		return new Vector3(
			Mathf.DeltaAngle(fromAngles.x, toAngles.x),
			Mathf.DeltaAngle(fromAngles.y, toAngles.y),
			Mathf.DeltaAngle(fromAngles.z, toAngles.z));
		}

	}
