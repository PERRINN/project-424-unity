//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2021 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Telemetry: modular telemetry system primarily for debug and diagnosis.

#if false

namespace VehiclePhysics
{

public class Telemetry
	{
	// ChannelGroup
	//----------------------------------------------------------------------------------------------

	// Telemetry channels are implemented as channel groups by vehicle subsystems and external components.
	// Use the method RegisterChannels to include them in the exposed telemetry.

	public class ChannelGroup
		{
		// Report the number of channels in this group

		public virtual int GetChannelCount ()
			{
			return 0;
			}

		// Report the frequency the PollValues method should be called for this group of channels

		public virtual PollFrequency GetPollFrequency ()
			{
			return PollFrequency.Normal;
			}

		// Return the information of each channel in this group for the given instance.
		// Fill-in the public fields of each ChannelInfo class in the array.
		// The length of the array will be the value returned by GetChannelCount.

		public virtual void GetChannelInfo (ChannelInfo[] channelInfo, Object instance)
			{
			}

		// Get the latest value in each channel for the given instance.
		// Values must be filled starting in the position 'index' for the first channel.
		// If no value is available for a channel, fill-in float.NaN.

		public virtual void PollValues (float[] values, int index, Object instance)
			{
			}
		}


	// Frequency values for each channel group.
	// Actual frequency values used by channels may vary up to ±50% depending on the FixedUpdate frequency.
	//
	//		VeryLow:	  1 Hz
	//		Low:		 10 Hz
	//		Normal:		 50 Hz	(default)
	//		High:		100 Hz
	//		Max:		Physics Frequency

	public enum PollFrequency { VeryLow, Low, Normal, High, Max, Count }


	// Nested classes and enums
	//----------------------------------------------------------------------------------------------

	// Semantic
	//
	// Semantic define channel's display format and ranges. Each semantic may take some ranges from the Specifications.

	public enum Semantic
		{
		Default,
		Custom,
		Ratio,
		SignedRatio,
		Speed,
		Acceleration,
		Weight,
		Distance,
		Gear,
		BankAngle,
		SteeringWheelAngle,
		BrakePressure,
		AngularVelocity,
		EngineRpm,
		EngineTorque,
		EnginePower,
		EngineLoad,
		FuelRate,
		WheelTorque,
		SuspensionTravel,
		SuspensionSpring,
		SuspensionDamper,
		SuspensionForce,
		SuspensionSpeed,
		SlipAngle,
		SlipVelocity,
		TireForce,
		TireFriction,

		Max
		};


	// SemanticInfo
	//
	// Range and display format for each semantic

	public class SemanticInfo
		{
		public float rangeMin = 0.0f;
		public float rangeMax = 1.0f;
		public float rangeQuantization = 0.1f;		// Grid, histogram distribution, etc

		public float displayMultiplier = 1.0f;
		public string displayFormat = "0.00";		// Display format. No units.
		public string displayFormatAlt = "0.00";	// Alternate format. No units. Used to display range limits.
		public string displayUnitsSuffix = "";		// Units. Includes any optional spaces.

		public string displayRangeMin { get; }
		public string displayRangeMax { get; }
		public string displayUnits { get; }

		public float range { get; }
		public float rangeBoundary { get; }

		public SemanticInfo ()
		public SemanticInfo (SemanticInfo original)

		public string FormatValue (float value)
		public string FormatValueNoUnits (float value)
		public string FormatValueAlt (float value)
		public string FormatValueWithUpdate (float value, bool updated)

		// Shortcut methods

		public void SetRangeAndFormat (float min, float max, string format, string unitsSuffix, float quantization = 0.1f, float multiplier = 1.0f, string alternateFormat = "0")
		public void SetUnits (string unitsSuffix, float multiplier = 1.0f)
		public void SetRange (float min, float max)
		public void SetFormat (string format, float quantization = 0.1f, string alternateFormat = "0")
		}


	// Specifications
	//
	// Vehicle specifications. Telemetry.specs should be filled by the vehicle controller implementation.

	public class Specifications									// Units		// Display
		{
		public float maxSpeed = 180 / 3.6f;						// m/s			// 180.0 km/h
		public int maxGearPosition = 5;							// -			// 5 gears
		public int minGearPosition = -1;						// -			// -1
		public float maxAcceleration = 9.81f * 2;				// m/s2			// 2.00 G
		public float maxEngineRpm = 8000.0f;					// rpm			// 8000 rpm
		public float maxEngineTorque = 200.0f;					// Nm			// 200.0 Nm
		public float minEngineTorque = -100.0f;					// Nm			// -100.0 Nm
		public float maxEnginePowerKw = 150.0f;					// kW			// 150.0 kW
		public float minEnginePowerKw = -90.0f;					// kW			// -90.0 kW
		public float maxEngineFuelRate = 10.0f;					// g/s			// 10.0 g/s
		public float maxSuspensionTravel = 0.25f;				// m			// 250 mm
		public float maxSuspensionSpring = 55000.0f;			// N/m			// 50.0 kN/m
		public float maxSuspensionDamper = 3000.0f;				// N/m/s		// 3.00 kN/m/s
		public float maxWheelTorque = 2000.0f;					// Nm			// 2000 Nm
		public float maxSuspensionLoad = 9.81f * 1000.0f;		// N			// 1000 kg
		public float maxTireForce = 10000.0f;					// N			// 10000 N
		}


	// ChannelInfo
	//
	// Information on a single channel.
	// All info is read/write except group (ChannelGroupInfo).

	public class ChannelInfo
		{
		public string name = "<no name>";
		public Semantic semantic = Semantic.Default;
		public SemanticInfo customSemantic = new SemanticInfo();

		public ChannelGroupInfo group { get; }
		public string fullName { get; }

		public ChannelInfo (ChannelGroupInfo channelGroup)

		// Shotcut methods

		public void SetNameAndSemantic (string channelName, Semantic channelSemantic, SemanticInfo customChannelSemantic = null)
		}


	// ChannelGroupInfo
	//
	// Information on a channel group.

	public class ChannelGroupInfo
		{
		// Public information

		public ChannelGroup channels { get; }
		public int channelCount { get; }
		public PollFrequency expectedFrequency { get; }
		public float actualFrequency { get; }
		public int updateInterval { get; }
		public string updateFrequencyLabel { get; }

		public int channelBaseIndex { get; }
		public Object instance;

		public System.Type type { get; }
		public int typeIndex { get; }
		public int typeCount { get; }
		public string typeName { get; }

		public ChannelGroupInfo (Telemetry telemetry, ChannelGroup group, int baseIndex, int groupTypeIndex)

		public void RefreshUpdateFrequency ()
		public void SetTypeCount (int count)
		}


	// DataRow
	//
	// A single row of telemetry information.
	// Updated every fixed timestep.
	// Latest data row is exposed as Telemetry.latest

	public class DataRow
		{
		public int frame;
		public double time;
		public double distance;
		public double totalTime;
		public double totalDistance;
		public int segmentNum;
		public int markers;
		public float markerTime;
		public bool markerFlag;
		public bool valuesUpdated;
		public float[] values;		// Current values
		public bool[] updated;		// Have the corresponding values been updated in this cycle?

		public DataRow (int channels = 0)

		public void CopyFrom (DataRow dataRow)
		public void Allocate (int channels)
		}


	// Exposed properties
	//----------------------------------------------------------------------------------------------

	// Current fixed-rate update frequency

	public float fixedUpdateFrequency { get; }

	// Latest telemetry data line available

	public DataRow latest { get; }

	// Vehicle specifications

	public Specifications specs { get; }

	// Channels and groups

	public IList<ChannelGroupInfo> channelGroups { get; }
	public IList<ChannelInfo> channels { get; }
	public IList<SemanticInfo> semantics { get; }
	public IDictionary<string, int> channelIndex { get; }

	// Settings

	public enum DistanceMode { Displacement, Gps }

	public DistanceMode distanceMode { get; set; } = DistanceMode.Gps;
	public bool adaptiveFrequency { get; set; } = true;
	public bool debugLog { get; set; }


	// Public API
	//----------------------------------------------------------------------------------------------


	public Telemetry (float fixedUpdateFrequency = 50.0f)

	public void SetStartPosition (Vector3 position)
	public void SetFixedUpdateFrequency (float fixedUpdateFrequency)
	public void Update (float fixedDeltaTime, Vector3 position, bool reposition, Vector3 positionOffset)

	// Configure range and display format for the built-in semantics using the specifications
	public void ApplySpecifications ()


	// Register and unregister channels

	public void Register<T> (Object instance) where T : ChannelGroup
	public void Unregister<T> (Object instance) where T : ChannelGroup
	public bool IsRegistered<T> (Object instance) where T : ChannelGroup

	// Mostly used by channel group themselves
	public int GetUpdateInterval (PollFrequency frequency)


	// Channels
	//----------------------------------------------------------------------------------------------

	// Get the channel index from channel name.
	//
	// Several instances of a channel may be referred buy adding an underscore and the zero-based
	// index of the instance to the name. Examples:
	//
	//	CompressionDiff			Get the first instance of CompressionDiff. Equivalent to CompressionDiff_0.
	//	CompressionDiff_0		Equivalent to CompressionDiff.
	//	CompressionDiff_1		Get the second instance of CompressionDiff.

	public int GetChannelIndex (string channelName)

	// Get formatted channel value and semantic info

	public string FormatChannelValue (int channelIndex, SemanticInfo semantic = null)
	public string FormatChannelValue (int channelIndex, float value, SemanticInfo semantic = null)
	public SemanticInfo GetChannelSemmantic (int channelIndex)
	public (float min, float max, float quantization) GetChannelRange (int channelIndex)


	// Telemetry commands and markers
	//----------------------------------------------------------------------------------------------

	// Reset time and/or distance.
	//
	// Zero values will be set in the next DataRow update, and time / distance will continue
	// counting from there.

	public void ResetTime (double offset = 0.0)
	public void ResetDistance (double offset = 0.0)

	// Segment number (i.e. lap number)

	public int segmentNumber { get; set; }


	// Markers
	//
	// Establish some marker or markers to be included in the next DataRow update.
	// Markers are preserved and accumulated until they make into a dataRow with valuesUpdated = true.
	// Then they are cleared for the next DataRow updates.
	//
	// Set a marker:
	//
	//		telemetry.SetMarker(telemetry.Marker.StartLinePass);
	//
	// Set a custom marker:
	//
	//		telemetry.SetMarker(telemetry.CustomMarker(0));
	//
	// Query if some marker data contains a marker:
	//
	//		bool isStartLine = telemetry.IsMarker(markerData, Telemetry.Marker.StartLinePas);
	//
	// Check if latest telemetry row contains an unique (non-duplicated) marker event:
	//
	//		bool isStartLine = telemetry.IsUniqueMarker(telemetry.latest, Telemetry.Marker.StartLinePas);


	public enum Marker
		{
		// First 16 markers reserved for predefined use

		// Reset markers, automatically set when these events occur

		TimeReset = 0,
		DistanceReset,
		Reposition,

		// Track markers

		StartLinePass,
		SectorLinePass,

		// Events

		Driver = 8,
		OffTrack,
		Contact,
		Impact,

		// Last 16 markers available for custom use.
		// Custom markers may be composed with CustomMarker(0..15).

		Custom = 16
		}


	public void SetMarker (Marker marker)
	public void SetMarker (int marker)
	public void SetMarkerTime (float time)
	public void SetMarkerFlag (bool flag)

	static public bool IsMarker (int markerData, Marker marker)
	static public bool IsMarker (int markerData, int marker)
	static public bool IsAnyMarker (int markerData, params Marker[] markers)
	static public bool IsAnyMarker (int markerData, params int[] markers)
	static public bool IsUniqueMarker (DataRow dataRow, int marker)

	static public int CustomMarker (int index)
	}

}

#endif