
// Integration with Dynisma motion platform simulator


using UnityEngine;
using VehiclePhysics.InputManagement;


namespace Perrinn424
{

public class DynismaInputProvider : MonoBehaviour
	{
	public DynismaInputDevice.Settings settings = new DynismaInputDevice.Settings();


	void OnEnable ()
		{
		if (InputManager.instance.RegisterDeviceProvider<DynismaDeviceProvider>())
			{
			DynismaDeviceProvider provider = InputManager.instance.GetDeviceProvider<DynismaDeviceProvider>();
			provider.settings = settings;
			}

		InputManager.instance.RefreshAllBindings();
		}


	void OnDisable ()
		{
		InputManager.instance.UnregisterDeviceProvider<DynismaDeviceProvider>();
		}
	}

}