

using UnityEngine;
using UniCAVE;
using EdyCommonTools;


namespace Perrinn424
{

public class DynismaTestMode : MonoBehaviour
	{
	public enum TestMode { None, Server, AllClients, Client1, Client2, Client3, Client4, Client5 };

	[HelpBox("Makes the current station to work as server or client by modifying the corresponding components and properties accordingly. AllClients enables all cameras in separate displays. Test Modes work when launching the scene in the Editor only. Modifying the mode in runtime has no effect.", HelpBoxMessageType.Info)]

	public TestMode testMode = TestMode.None;


	void Awake ()
		{
		// Test modes are enabled while in Editor only

		if (!Application.isEditor) return;

		// Apply selected test mode, if any.

		if (testMode == TestMode.Server)
			{
			// Force server mode in this station

			UCNetworkManager networkManager = FindObjectOfType<UCNetworkManager>();

			if (networkManager != null)
				{
				MachineName currentMachine = ScriptableObject.CreateInstance<MachineName>();
				currentMachine.Name = Util.GetMachineName();
				networkManager.headMachineAsset = currentMachine;
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
	}
}