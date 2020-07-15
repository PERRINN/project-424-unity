//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using System;


namespace VehiclePhysics.Timing.EverLaps
{
	public enum MsgType
	{
		Connect,			// -> Initialize a connection from the client (VPP) to the server (Everlaps)
		Passing,			// -> When a passing occurs
		Disconnect,			// -> Client closes connection with server. Can include a reason.
		DecoderTimeRequest, // <- Server (Everlaps) request the current time to the client (VPP)
		DecoderTime,		// -> Client sends the current time as requested by server
		InvalidateLap		// -> Invalidates current lap
	}

	public enum Role
	{
		SF,			// Start-Finish line. ID = 0
		Sector,		// Partial sector. ID = 1, 2, 3...
		PitIn,		// Pit enter. ID = any
		PitOut,		// Pit exit. ID any matching the PitIn ID
		Trap		// Passing trap. ID = any
	}

	abstract public class BaseVirtualMsg
	{
		protected BaseVirtualMsg(MsgType type) { Type = type; }

		public MsgType Type;
	}

	public class PassingMsg: BaseVirtualMsg
	{
		public PassingMsg(): base(MsgType.Passing) {}

		public uint transponder;	// Transponder of the car
		public long timeMS;			// Time of passing
		public int id;				// id of the loop
		public float speed;			// m/s
	}

	public class DisconnectMsg: BaseVirtualMsg
	{
		public DisconnectMsg(): base(MsgType.Disconnect) {}

		public int reason;
	}

	public class DecoderTimeRequestMsg: BaseVirtualMsg
	{
		public DecoderTimeRequestMsg(): base(MsgType.DecoderTimeRequest) {}
	}

	public class DecoderTimeMsg: BaseVirtualMsg
	{
		public DecoderTimeMsg(): base(MsgType.DecoderTime) {}

		public long timeMS;
	}
}
