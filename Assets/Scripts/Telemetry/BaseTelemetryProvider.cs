using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public abstract class BaseTelemetryProvider<TComponent,TTelemetry> : VehicleBehaviour where TComponent:UnityEngine.Object where TTelemetry: Telemetry.ChannelGroup
	{
		public bool emitTelemetry = true;

		[SerializeField]
		private TComponent component;

		public override void OnEnableVehicle()
		{
			if (component == null)
			{
				component = vehicle.GetComponentInChildren<TComponent>();

				if (component == null)
				{
					Debug.LogError($"Missing {nameof(TComponent)}. Component disabled");
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
			vehicle.telemetry.Register<TTelemetry>(component);
		}


		public override void UnregisterTelemetry()
		{
			vehicle.telemetry.Unregister<TTelemetry>(component);
		}
	}
}