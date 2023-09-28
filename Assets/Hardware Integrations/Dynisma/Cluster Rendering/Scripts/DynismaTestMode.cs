
// Script to force this station to work as a server, as a client, or as displaying all clients.
// Execution order must be set before default time so its Awake is called before the network initializes.


using UnityEngine;
using UniCAVE;
using EdyCommonTools;
using VehiclePhysics;


namespace Perrinn424
{

public class DynismaTestMode : MonoBehaviour
	{
	public enum TestMode { None, Server, AllClients, Client1, Client2, Client3, Client4, Client5 };

	[HelpBox("Makes the current station to work as server or client by modifying the corresponding components and properties accordingly. AllClients enables all cameras in separate displays.", HelpBoxMessageType.Info)]

	public TestMode testMode = TestMode.None;
	public bool loadReplayInServer = false;
	public VPReplayAsset replayAsset;


	bool m_initializeReplay = false;
	Perrinn424CarController m_carController;


	void Awake ()
		{
		// Test modes are enabled while in Editor only

		// if (!Application.isEditor) return;

		// Apply selected test mode, if any.

		m_carController = FindObjectOfType<Perrinn424CarController>();

		if (testMode == TestMode.Server)
			{
			// Force server mode in this station

			UCNetworkManager networkManager = FindObjectOfType<UCNetworkManager>();

			if (networkManager != null)
				{
				MachineName currentMachine = ScriptableObject.CreateInstance<MachineName>();
				currentMachine.Name = Util.GetMachineName();
				networkManager.headMachineAsset = currentMachine;
				m_initializeReplay = true;
				}
			else
				{
				Debug.LogWarning("Error configuring station as Server: Couldn't find the Network Manager");
				}
			}
		else if (testMode == TestMode.AllClients)
			{
			// Enable all clients, each one in a display

			Perrinn424ClusterManager clusterManager = FindObjectOfType<Perrinn424ClusterManager>();

			if (clusterManager != null)
				{
				clusterManager.enableTestMode = true;
				clusterManager.testStation = Perrinn424ClusterManager.TestStation.AllClients;
				}
			}
		else if (testMode >= TestMode.Client1 && testMode <= TestMode.Client5)
			{
			// Enable the given client

			Perrinn424ClusterManager clusterManager = FindObjectOfType<Perrinn424ClusterManager>();

			if (clusterManager != null)
				{
				clusterManager.enableTestMode = true;
				clusterManager.testStation = Perrinn424ClusterManager.TestStation.Client1 + (int)(testMode - TestMode.Client1);
				}
			}
		}


	void Update ()
		{
		// Initialize replay if specified.

		if (m_initializeReplay)
			{
			m_initializeReplay = false;

			if (loadReplayInServer && replayAsset != null && m_carController != null)
				{
				if (m_carController.initialized)
					{
					VPReplay replay = m_carController.GetComponentInChildren<VPReplay>(true);
					if (replay != null)
						{
						replay.LoadReplayFromAsset(replayAsset);
						replay.onEnableAction = VPReplay.OnEnableAction.None;
						replay.endOfPlayback = VPReplay.EndOfPlayback.Pause;
						replay.gameObject.SetActive(true);
						replay.enabled = true;
						replay.Pause(VPReplay.PauseMode.AtBegin);
						replay.Jump(1);
						replay.Jump(0);
						}

					VPReplayController replayController = m_carController.GetComponentInChildren<VPReplayController>(true);
					if (replayController != null)
						{
						replayController.enableShortcuts = true;
						replayController.showPanel = true;
						replayController.recordKey = KeyCode.None;
						replayController.saveReplayKey = KeyCode.None;
						}
					}
				else
					{
					// Everything in order but the car is not yet initialized. Keep trying.
					m_initializeReplay = true;
					}
				}
			}
		}
	}
}