
// Manager for cluster rendering setups


using System;
using UnityEngine;
using UniCAVE;
using Mirror;


namespace Perrinn424
{

public class Perrinn424ClusterManager : NetworkBehaviour
	{
	public Station server;
	public Station[] clients = new Station[0];

	[Header("Test Mode")]
	public bool enableTestMode = false;
	public TestStation testStation = TestStation.Server;

	public enum TestStation { Server, Client1, Client2, Client3, Client4, Client5, Client6, Client7, Client8, Client9, Client10 }

	bool m_testMode;
	TestStation m_testStation;


	[Serializable]
	public class Station
		{
        public MachineName machineName;
		public Camera camera;
		}


	void Awake ()
		{
		// Everything disabled on startup
		DisableAll();
		}


	void OnEnable ()
		{
		m_testMode = false;
		m_testStation = testStation;

		if (!enableTestMode)
			EnableNetworkStation(Util.GetMachineName());
		}


	public override void OnStartServer ()
		{
		if (NetworkManager.DebugInfoLevel >= 2)
			Debug.Log($"ClusterManager SERVER: {Util.GetMachineName()}");
		}


	public override void OnStartClient ()
		{
		if (NetworkManager.DebugInfoLevel >= 2)
			Debug.Log($"RenderClient CLIENT: {Util.GetMachineName()}");
		}


	void Update ()
		{
		if (enableTestMode != m_testMode || testStation != m_testStation)
			{
			if (enableTestMode)
				{
				DisableAll();
				if ((int)testStation > clients.Length)
					testStation = (TestStation)clients.Length;

				if (testStation == TestStation.Server)
					SetActive(server, true);
				else
					SetActive(clients[(int)testStation - 1], true);
				}
			else
			if (m_testMode)
				{
				EnableNetworkStation(Util.GetMachineName());
				}

			m_testMode = enableTestMode;
			m_testStation = testStation;
			}
		}


	//------------------------------------------------------------------------------------------------------


	void EnableNetworkStation (string machineName)
		{
		SetActive(server, ShouldBeActive(server, machineName));
		foreach (Station s in clients)
			SetActive(s, ShouldBeActive(s, machineName));
		}


	bool ShouldBeActive (Station station, string machineName)
		{
		if (station == null && station.machineName == null)
			return false;

		return station.machineName.Name == machineName;
		}


	void SetActive (Station station, bool active)
		{
		if (station != null && station.camera != null)
			station.camera.gameObject.SetActive(active);
		}


	void DisableAll ()
		{
		SetActive(server, false);
		foreach (Station s in clients)
			SetActive(s, false);
		}
	}

}