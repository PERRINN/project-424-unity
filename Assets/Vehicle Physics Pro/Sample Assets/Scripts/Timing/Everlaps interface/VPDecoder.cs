//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using EdyCommonTools;


namespace VehiclePhysics.Timing.EverLaps
{

public class VPDecoder : MonoBehaviour
	{
	public string server;
	public int port = 12345;
	public int listenerPort = 12345;
	public bool debugInfo = false;

	// Mandatory: one start-finish loop. Other SF loops may be located inside the pits.
	// All other elements are optional.

	[Header("Track Layout")]
	public List<VPLoop> startFinish;

	// Track sequence is used for monitoring car positions and time intervals.

	[FormerlySerializedAs("trackSequence")]
	public List<VPLoop> sequence;

	// Sectors generate the sector times.
	// Sectors don't monitor car positions nor time intervals. Set a track sequence
	// for that, even if the loops are the same as in sectors.

	public List<VPLoop> sectors;

	// Detecting pits require at least a Pit-In loop.
	// Any other loop not marked as "Inside Pit" will trigger the pit-out event.
	// Pit times are generated between the Pit-In and Pit-Out loops.

	public List<TrackPit> pits;

	// Speed traps require the distance among the loops for calculating the speed.

	public List<SpeedTrap> speedTraps;


	[Serializable]
	public class TrackPit
		{
		public VPLoop inLoop;
		public VPLoop outLoop;
		}

	[Serializable]
	public class SpeedTrap
		{
		public VPLoop inLoop;
		public VPLoop outLoop;
		public float length;
		}


	// Private

	UdpSender m_sender;

	UdpConnection m_udp = new UdpConnection();
	UdpListenThread m_thread = new UdpListenThread();

	long m_startTime = 0;
	float m_fixedTime = 0.0f;

	bool m_acknowledged = false;


	void OnEnable ()
		{
        m_sender = new UdpSender(server, port);
		m_udp.StartConnection(listenerPort);
		// m_udp.SetDestination(server, port);

		m_thread.Start(m_udp, OnReceiveData);
		m_acknowledged = false;
		}


	void Start ()
		{
		// Here Time.time is zero

		m_startTime = DateTime.Now.Ticks / 10000L;
		m_fixedTime = 0.0f;
		SendConnectAndLayout();
		SendTime();
		}


	void OnDisable ()
		{
		m_sender.Close();
		m_thread.Stop();
		m_udp.StopConnection();
		}


	void OnReceiveData ()
		{
		if (debugInfo)
			Debug.Log("Everlaps talked! Yes, my master!");

		m_udp.GetMessageString();
		SendTime();

		m_acknowledged = true;
		}


	void FixedUpdate ()
		{
		m_fixedTime = Time.time;

		if (!m_acknowledged)
			SendConnectAndLayout();
		}


	void SendTime ()
		{
		DecoderTimeMsg packet = new DecoderTimeMsg
			{
			timeMS = m_startTime + Mathf.RoundToInt(m_fixedTime * 1000)
			};

		string json = JsonUtility.ToJson(packet);
		byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(json);
		// m_udp.SendMessageBinary(bytesToSend);
		m_sender.SendSync(bytesToSend);
		}


	// Private class for serializing the track layout to JSON as Connect message.
	// Member fields are named as they should appear in the JSON file.

	private class TrackLayout
		{
		[Serializable]
		public class Loop
			{
			public string Name;
			public int Id;
			public bool InsidePit;

			public Loop (VPLoop loop)
				{
				Name = loop.name;
				Id = loop.id;
				InsidePit = loop.insidePit;
				}
			}

		[Serializable]
		public class Pit
			{
			public int In;
			public int Out;

			public Pit (TrackPit pit)
				{
				In = pit.inLoop == null? -1 : pit.inLoop.id;
				Out = pit.outLoop == null? -1 : pit.outLoop.id;
				}
			}

		[Serializable]
		public class Trap
			{
			public int In;
			public int Out;
			public float Length;

			public Trap (SpeedTrap trap)
				{
				In = trap.inLoop == null? -1 : trap.inLoop.id;
				Out = trap.outLoop == null? -1 : trap.outLoop.id;
				Length = trap.length;
				}
			}


		public MsgType Type = MsgType.Connect;
		public List<Loop> Loops = new List<Loop>();

		public List<int> StartFinish = new List<int>();
		public List<int> Sequence = new List<int>();
		public List<int> Sectors = new List<int>();
		public List<Pit> Pits = new List<Pit>();
		public List<Trap> Traps = new List<Trap>();
		}


	void SendConnectAndLayout ()
		{
		TrackLayout layout = new TrackLayout();
		List<VPLoop> loops = new List<VPLoop>();

		// Populate the loop lists.
		// Create a list of unique loops at the same time.

		foreach (VPLoop loop in startFinish)
			{
			if (loop != null)
				{
				if (!loops.Contains(loop)) loops.Add(loop);
				layout.StartFinish.Add(loop.id);
				}
			}

		foreach (VPLoop loop in sequence)
			{
			if (loop != null)
				{
				if (!loops.Contains(loop)) loops.Add(loop);
				layout.Sequence.Add(loop.id);
				}
			}

		foreach (VPLoop loop in sectors)
			{
			if (loop != null)
				{
				if (!loops.Contains(loop)) loops.Add(loop);
				layout.Sectors.Add(loop.id);
				}
			}

		foreach (TrackPit pit in pits)
			{
			if (pit != null)
				{
				if (pit.inLoop != null && !loops.Contains(pit.inLoop)) loops.Add(pit.inLoop);
				if (pit.outLoop != null && !loops.Contains(pit.outLoop)) loops.Add(pit.outLoop);
				layout.Pits.Add(new TrackLayout.Pit(pit));
				}
			}

		foreach (SpeedTrap trap in speedTraps)
			{
			if (trap != null)
				{
				if (trap.inLoop != null && !loops.Contains(trap.inLoop)) loops.Add(trap.inLoop);
				if (trap.outLoop != null && !loops.Contains(trap.outLoop)) loops.Add(trap.outLoop);
				layout.Traps.Add(new TrackLayout.Trap(trap));
				}
			}

		// Populate the list of unique loops

		foreach (VPLoop loop in loops)
			layout.Loops.Add(new TrackLayout.Loop(loop));


		/*
		List<VPLoop> loops = new List<VPLoop>();

		// Create a list of unique loops

		foreach (TrackElement element in elements)
			{
			if (element.passLoop != null && !loops.Contains(element.passLoop)) loops.Add(element.passLoop);
			if (element.inLoop != null && !loops.Contains(element.inLoop)) loops.Add(element.inLoop);
			if (element.outLoop != null && !loops.Contains(element.outLoop)) loops.Add(element.outLoop);
			}

		// Create the layout and populate the lists

		TrackLayout layout = new TrackLayout();

		foreach (VPLoop loop in loops)
			layout.Loops.Add(new TrackLayout.Loop(loop));

		foreach (TrackElement element in elements)
			layout.Layout.Add(new TrackLayout.Element(element));
		*/

		// Serialize and send

		string json = JsonUtility.ToJson(layout);
		if (debugInfo)
			Debug.Log(json);
		byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(json);
		m_sender.SendSync(bytesToSend);
		}



	public void Pass (VPLoop loop, VPTransponder transponder, float time, float speed)
		{
		PassingMsg packet = new PassingMsg
			{
			transponder = transponder.id,
			timeMS = m_startTime + Mathf.RoundToInt(time * 1000),
			id = loop.id,
			speed = speed,
			};

		if (debugInfo)
			Debug.Log("Pass! Loop: " + loop.name + " (" + packet.id + ") Transponder: " + packet.transponder + " Time: " + time + " Speed: " + (packet.speed * 3.6f));

		string json = JsonUtility.ToJson(packet);
		byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(json);

		m_sender.SendSync(bytesToSend);
		// m_udp.SendMessageBinary(bytesToSend);
		}


	public void InvalidPass (VPTransponder transponder, float time, float speed)
		{
		// Reasons:
		//  - Offlimits
		//  - Shortcut
		//  - SpeedLimit
		//  - Replay
		//  - Other
		}
	}

}
