//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;


namespace VehiclePhysics.UI
{

public class DrivingAidsPanel : MonoBehaviour
	{
	public VehicleBase vehicle;

	[Header("UI")]
	public GameObject autoShiftEnabled;
	public GameObject diffLockEnabled;
	public GameObject absOffEnabled;

	public enum DefaultAction { DoNothing, Enable, Disable };

	[Header("Defaults")]
	public DefaultAction autoShift = DefaultAction.Enable;
	public DefaultAction diffLock = DefaultAction.DoNothing;
	public DefaultAction absOff = DefaultAction.DoNothing;


	// UI event receivers


	public void AutoShiftClick ()
		{
		if (vehicle != null)
			ConfigureAutoShift(!AutoShiftEnabled());
		}


	public void DiffLockClick ()
		{
		if (vehicle != null)
			ConfigureDiffLock(!DiffLockEnabled());
		}


	public void AbsOffClick ()
		{
		if (vehicle != null)
			ConfigureAbsOff(!AbsOffEnabled());
		}


	public void ManettinoDialChanged (int position)
		{
		#if !VPP_LIMITED
		if (vehicle != null)
			{
			VPSettingsSwitcher settings = vehicle.GetComponent<VPSettingsSwitcher>();

			if (settings != null)
				{
				settings.selectedGroup = position;
				settings.Refresh();
				}
			}
		#endif
		}


	// ConfigureDefaults should be called externally when the UI is initialized

	public void ConfigureDefaults ()
		{
		if (vehicle != null)
			{
			if (autoShift != DefaultAction.DoNothing) ConfigureAutoShift(autoShift == DefaultAction.Enable);
			if (diffLock != DefaultAction.DoNothing) ConfigureDiffLock(diffLock == DefaultAction.Enable);
			if (absOff != DefaultAction.DoNothing) ConfigureAbsOff(absOff == DefaultAction.Enable);
			}
		}


	// Component methods


	void Update ()
		{
		// Update visual states

		if (vehicle != null)
			{
			if (autoShiftEnabled != null) autoShiftEnabled.SetActive(AutoShiftEnabled());
			if (diffLockEnabled != null) diffLockEnabled.SetActive(DiffLockEnabled());
			if (absOffEnabled != null) absOffEnabled.SetActive(AbsOffEnabled());
			}
		}


	// Helper methods
	// Require vehicle != null


	bool AutoShiftEnabled ()
		{
		return vehicle.data.Get(Channel.Settings, SettingsData.AutoShiftOverride) == (int)Gearbox.AutoShiftOverride.ForceAutoShift;
		}


	bool DiffLockEnabled ()
		{
		return vehicle.data.Get(Channel.Settings, SettingsData.DifferentialLock) == (int)Driveline.Override.ForceLocked;
		}


	bool AbsOffEnabled ()
		{
		return vehicle.data.Get(Channel.Settings, SettingsData.AbsOverride) == (int)Brakes.AbsOverride.ForceDisabled;
		}


	void ConfigureAutoShift (bool enable)
		{
		if (enable)
			vehicle.data.Set(Channel.Settings, SettingsData.AutoShiftOverride, (int)Gearbox.AutoShiftOverride.ForceAutoShift);
		else
			vehicle.data.Set(Channel.Settings, SettingsData.AutoShiftOverride, (int)Gearbox.AutoShiftOverride.None);
		}


	void ConfigureDiffLock (bool enable)
		{
		if (enable)
			vehicle.data.Set(Channel.Settings, SettingsData.DifferentialLock, (int)Driveline.Override.ForceLocked);
		else
			vehicle.data.Set(Channel.Settings, SettingsData.DifferentialLock, (int)Driveline.Override.None);
		}


	void ConfigureAbsOff (bool enable)
		{
		if (enable)
			vehicle.data.Set(Channel.Settings, SettingsData.AbsOverride, (int)Brakes.AbsOverride.ForceDisabled);
		else
			vehicle.data.Set(Channel.Settings, SettingsData.AbsOverride, (int)Brakes.AbsOverride.None);
		}
	}

}