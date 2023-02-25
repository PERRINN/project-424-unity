
// Integration with Dynisma motion platform simulator


using UnityEngine;
using VehiclePhysics.InputManagement;


namespace Perrinn424
{

public class DynismaDeviceProvider : InputDeviceProvider
	{
	public DynismaInputDevice.Settings settings = new DynismaInputDevice.Settings();


	InputDevice m_device = null;
	DeviceDefinition m_deviceDefinition = new DeviceDefinition()
		{
		name = "Dynisma Remote Input"
		};


	public override bool Initialize ()
		{
		UpdateDeviceDefinition();
		return true;
		}


	public override DeviceDefinition[] GetDeviceList ()
		{
		return new DeviceDefinition[] { m_deviceDefinition };
		}


	public override InputDevice GetDevice (DeviceDefinition definition, DeviceSearchMode searchMode = DeviceSearchMode.Default)
		{
		// Verify if the definition matches our device

		if (searchMode == DeviceSearchMode.Default)
			{
			if (!definition.Match(m_deviceDefinition))
				return null;
			}
		else
		if (searchMode != DeviceSearchMode.ByName || !definition.MatchName(m_deviceDefinition))
			return null;

		// Return our unique device

		return GetDeviceByIndex(0);
		}


	public override InputDevice GetDeviceByIndex (int index)
		{
		if (index != 0) return null;

		// Create an instance of the device, open and return it

		if (m_device == null)
			{
			UpdateDeviceDefinition();
			m_device = new DynismaInputDevice()
				{
				definition = m_deviceDefinition,
				settings = settings,
				};

			m_device.Open();
			}

		return m_device;
		}


	public override DeviceInfo GetDeviceInfo (int index)
		{
		if (index != 0)
			return new DeviceInfo { name = "Bad device index" };

		return new DeviceInfo
			{
			name = "Dynisma Remote Input",
			axes = 3,
			buttons = 46,
			dpads = 0,
			forceFeedback = false,
			};
		}


	public override void Update ()
		{
		if (m_device != null)
			m_device.Update();
		}


	public override void Release ()
		{
		if (m_device != null)
			{
			m_device.Close();
			m_device = null;
			}
		}


	public override void OpenAllDevices ()
		{
		GetDeviceByIndex(0);
		}


	public override void CloseAllDevices ()
		{
		if (m_device != null)
			{
			m_device.Close();
			m_device = null;
			}
		}


	public override void CloseUnusedDevices ()
		{
		if (m_device != null && !InputManager.instance.DeviceHasBindings(m_deviceDefinition))
			{
			m_device.Close();
			m_device = null;
			}
		}


	public override void TakeControlSnapshot ()
		{
		if (m_device != null)
			m_device.TakeControlSnapshot();
		}


	public override bool DetectPressedControl (ref ControlDefinition control, ref DeviceDefinition device)
		{
		if (m_device == null) return false;

		if (m_device.DetectPressedControl(ref control))
			{
			device = m_device.definition;
			return true;
			}

		return false;
		}


	void UpdateDeviceDefinition ()
		{
		m_deviceDefinition.product = $"Port:{settings.listeningPort}";
		}

	}

}