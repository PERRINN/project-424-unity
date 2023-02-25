
using UnityEngine;


namespace Perrinn424
{

public class DisableOnTimeout : MonoBehaviour
	{
	public float timeout = 10.0f;
	public bool unscaledTime = true;

	float m_startTime;


	void OnEnable ()
		{
		float time = unscaledTime? Time.unscaledTime : Time.time;
		m_startTime = time;
		}


	void Update ()
		{
		float time = unscaledTime? Time.unscaledTime : Time.time;

		if (time - m_startTime >= 10.0f)
			gameObject.SetActive(false);
		}
	}
}
