using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class Perrin424AutopilotTelemetryProvider : VehicleBehaviour
    {
        public bool emitTelemetry = true;

		[SerializeField]
		private Autopilot autopilot;

		public override void OnEnableVehicle()
		{
			if (autopilot == null)
			{
				autopilot = vehicle.GetComponentInChildren<Autopilot>();

				if (autopilot == null)
				{
					Debug.LogError("Missing Autopilot. Component disabled");
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
			vehicle.telemetry.Register<AutopilotTelemetry>(autopilot);
		}


		public override void UnregisterTelemetry()
		{
			vehicle.telemetry.Unregister<AutopilotTelemetry>(autopilot);
		}

		public class AutopilotTelemetry : Telemetry.ChannelGroup
		{

			private Autopilot autopilot;

			public override int GetChannelCount()
			{
                return 5;
            }

            public override void GetChannelInfo(Telemetry.ChannelInfo[] channelInfo, Object instance)
            {
				autopilot = (Autopilot)instance;
				
				Telemetry.SemanticInfo errorSemantic = new Telemetry.SemanticInfo();
				errorSemantic.SetRangeAndFormat(-5, 5, "0.#E+00", " m");

				float range = Mathf.Max(autopilot.maxForceP, autopilot.maxForceD);
				Telemetry.SemanticInfo pidSemantic = new Telemetry.SemanticInfo(); 
				pidSemantic.SetRangeAndFormat(-range, range, "0.00", " N");

				channelInfo[0].SetNameAndSemantic("Error", Telemetry.Semantic.Custom, errorSemantic);
				channelInfo[1].SetNameAndSemantic("P", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[2].SetNameAndSemantic("I", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[3].SetNameAndSemantic("D", Telemetry.Semantic.Custom, pidSemantic);
				channelInfo[4].SetNameAndSemantic("PID", Telemetry.Semantic.Custom, pidSemantic);
			}

			public override float GetPollFrequency()
			{
				return 50.0f;
			}

            public override void PollValues(float[] values, int index, Object instance)
            {
				values[index + 0] = autopilot.Error;
				values[index + 1] = autopilot.P;
				values[index + 2] = autopilot.I;
				values[index + 3] = autopilot.D;
				values[index + 4] = autopilot.PID;
            }
        }

	}
}
