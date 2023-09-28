
// Manager for cluster rendering setups


using System;
using UnityEngine;
using UnityEngine.UI;
using EdyCommonTools;
using UniCAVE;
using Mirror;


namespace Perrinn424
{

public class Perrinn424ClusterManager : MonoBehaviour
	{
	[Header("Stations")]
	public Camera serverCamera;
	public Station[] clients = new Station[0];

	[Header("UI Material")]
	[HelpBox("Canvases will be assigned to the corresponding camera and their components will use the given UI material. Activation state won't be changed here.")]
	public Material uiMaterial;
	public Canvas[] serverCanvasList = new Canvas[0];
	public Canvas[] clientCanvasList = new Canvas[0];

	[Header("On unknown station")]
	[HelpBox("Keep these components and game objects enabled or disabled on unknown stations. They might have been modified by other managers.")]
	public Behaviour[] enableComponents = new Behaviour[0];
	public GameObject[] enableGameObjects = new GameObject[0];
	public Behaviour[] disableComponents = new Behaviour[0];
	public GameObject[] disableGameObjects = new GameObject[0];

	[Header("Test Mode")]
	public bool enableTestMode = false;
	public TestStation testStation = TestStation.Server;
	public enum TestStation { Server, AllClients, Client1, Client2, Client3, Client4, Client5, Client6, Client7, Client8, Client9, Client10 }

	bool m_testMode;
	bool m_unknownStation;
	TestStation m_testStation;
	Station m_serverStation = new Station();


	[Serializable]
	public class Station
		{
        public MachineName machineName;
		public Camera camera;

		public string name => machineName != null? machineName.Name : "";
		}


	void Awake ()
		{
		// Everything disabled on startup
		DisableAll();
		}


	void OnEnable ()
		{
		UCNetworkManager networkManager = FindObjectOfType<UCNetworkManager>();
		if (networkManager == null)
			{
			Debug.LogError("Perrinn424ClusterManager: Network Manager not found. Component disabled.");
			enabled = false;
			return;
			}

		m_serverStation.camera = serverCamera;
		m_serverStation.machineName = networkManager.headMachineAsset;

		m_testMode = false;
		m_testStation = testStation;

		if (!enableTestMode)
			EnableNetworkStation(Util.GetMachineName());
		}


	void Update ()
		{
		// Handle test mode

		if (enableTestMode != m_testMode || testStation != m_testStation)
			{
			if (enableTestMode)
				{
				if (((int)testStation - (int)TestStation.Client1) >= clients.Length)
					testStation = TestStation.Server;

				if (testStation == TestStation.AllClients)
					{
					// Enable all stations and make them render in separate displays

					for (int i = 0, c = clients.Length; i < c; i++)
						{
						Station s = clients[i];
						SetActive(s, true, clientCanvasList);
						s.camera.targetDisplay = i;
						}

					// Ensure UIs are displayed in the center display

					int centerDisplay = clients.Length / 2;
					SetActive(clients[centerDisplay], true, clientCanvasList);
					}
				else
					{
					string stationName = testStation == TestStation.Server? m_serverStation.name : clients[(int)(testStation - TestStation.Client1)].name;
					EnableNetworkStation(stationName);
					}
				}
			else
			if (m_testMode)
				{
				EnableNetworkStation(Util.GetMachineName());
				}

			m_testMode = enableTestMode;
			m_testStation = testStation;
			}

		// Keep elements enabled or disabled on unknown stations

		if (m_unknownStation)
			{
			foreach (Behaviour c in enableComponents)
				{
				if (!c.enabled) c.enabled = true;
				}

			foreach (GameObject g in enableGameObjects)
				{
				if (!g.activeSelf) g.SetActive(true);
				}

			foreach (Behaviour c in disableComponents)
				{
				if (c.enabled) c.enabled = false;
				}

			foreach (GameObject g in disableGameObjects)
				{
				if (g.activeSelf) g.SetActive(false);
				}
			}
		}


	//------------------------------------------------------------------------------------------------------


	void EnableNetworkStation (string machineName)
		{
		bool serverActive = ShouldBeActive(m_serverStation, machineName);
		SetActive(m_serverStation, serverActive, serverCanvasList);

		bool anyStationActive = serverActive;
		foreach (Station s in clients)
			{
			bool stationActive = ShouldBeActive(s, machineName);
			anyStationActive |= stationActive;
			SetActive(s, stationActive, clientCanvasList);
			}

		m_unknownStation = !anyStationActive;
		}


	bool ShouldBeActive (Station station, string machineName)
		{
		if (station == null || station.machineName == null)
			return false;

		return station.machineName.Name == machineName.ToUpper();
		}


	void SetActive (Station station, bool active, Canvas[] canvasList = null)
		{
		if (station != null && station.camera != null)
			{
			station.camera.gameObject.SetActive(active);

			if (active && canvasList != null)
				AssignCanvasList(station.camera, canvasList);
			}
		}


	void AssignCanvasList (Camera camera, Canvas[] canvasList)
		{
		if (camera == null) return;

		foreach (Canvas canvas in canvasList)
			{
			if (canvas == null) continue;

			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = camera;
			canvas.planeDistance = 1.0f;

			if (uiMaterial != null)
				{
				Graphic[] graphics = canvas.gameObject.GetComponentsInChildren<Graphic>(true);

				foreach (Graphic graphic in graphics)
					graphic.material = uiMaterial;
				}
			}
		}


	void DisableAll ()
		{
		SetActive(m_serverStation, false);
		foreach (Station s in clients)
			SetActive(s, false);
		}
	}

}