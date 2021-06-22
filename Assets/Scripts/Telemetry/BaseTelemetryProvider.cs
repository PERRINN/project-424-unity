using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public abstract class BaseTelemetryProvider<T,R> : VehicleBehaviour where T:UnityEngine.Object where R: Telemetry.ChannelGroup
	{
		public bool emitTelemetry = true;

		[SerializeField]
		private T component;

		public override void OnEnableVehicle()
		{
			if (component == null)
			{
				component = vehicle.GetComponentInChildren<T>();

				if (component == null)
				{
					Debug.LogError($"Missing {nameof(T)}. Component disabled");
					enabled = false;
				}
			}
		}

		public override bool EmitTelemetry()
		{
			return emitTelemetry;
		}

		public override void RegisterTelemetry()
		{
			vehicle.telemetry.Register<R>(component);
		}


		public override void UnregisterTelemetry()
		{
			vehicle.telemetry.Unregister<R>(component);
		}
	}
}