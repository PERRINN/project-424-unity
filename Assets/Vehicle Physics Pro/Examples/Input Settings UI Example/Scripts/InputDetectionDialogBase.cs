//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// InputDetectionDialogBase: common elements for input detection dialogs


using UnityEngine;
using VehiclePhysics.InputManagement;
using System;
using System.Collections.Generic;


namespace VehiclePhysics.UI
{

public class InputDetectionDialogBase : MonoBehaviour
	{
	[NonSerialized] public bool assigned = false;
	[NonSerialized] public DeviceDefinition device = new DeviceDefinition();
	[NonSerialized]	public ControlDefinition control = new ControlDefinition();
	[NonSerialized] public List<(string key, string value)> settings = new List <(string key, string value)>();
	}
}
