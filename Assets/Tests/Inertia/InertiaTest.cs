

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
	Vector3 m_lastAngularVelocity;
	Inertia m_inertiaHelper = new Inertia();
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
		DebugUtility.DrawCrossMark(m_rigidbody.worldCenterOfMass, GColor.brown, 0.2f);
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

		m_text = $"Frame / Time:         #{m_internalFrame,-3} {m_internalTime.ToString("0.000")}\n\nAngular Velocity:     {m_lastAngularVelocity.ToString("0.00000")}\nAngular Acceleration: {angularAcceleration.ToString("0.00000")}\n";

		if (m_internalTime < forceStart * 2 + forceDuration)
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


	}
