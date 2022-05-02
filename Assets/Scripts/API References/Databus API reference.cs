//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2022 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Databus: Generic communication bus inspired in the ODB-II standard
//
// http://en.wikipedia.org/wiki/OBD-II_PIDs
//
// Features:
//
//  - Inmmediate access O(1)
//	- Direct, non-blocking read/write access from all clients
//	- Transport only: no error control, no range checking
//	- Exact data only: integers. Float values are represented as fixed-resolution decimals (ie: 1.0 = 10000)
//	- NaN is gracefully supported as integer: (int)NaN = -2147483647
//	- Extensible
//
// Note: The write only / read only hints below refer to intended usage of the bus as seen from
// outside of the vehicle controller. Internally values are all read/write, but using them in a
// different way than specified might have unexpected results.


#if false


namespace VehiclePhysics
{

public struct Channel
	{
	public const int Input			= 0;		// Write only. States of the standard input elements.
	public const int RaceInput		= 1;		// Write only. Inputs for racing and competition features.
	public const int Vehicle		= 2;		// Read only. Standard values of the vehicle.
	public const int Settings		= 3;		// Read/write. Common configuration settings. Optionally implemented.
	public const int Custom			= 4;		// Custom vehicle implementations may use this bus (100 values) at their convenience.

	public const int Count			= 5;
	}


// Input channel (write only)

public struct InputData				// PID			DESCRIPTION							RESOLUTION		EXAMPLE
	{
	public const int Steer 			=  0;		// Steering wheel position in ratio			10000		-10000 (full left), 0 (center), +10000 (full right)
	public const int Throttle 		=  1;		// Throttle pedal position in ratio			10000		10000 (= 1.0 = 100%)
	public const int Brake			=  2;		// Brake pedal position in ratio			10000       5000 (= 0.5 = 50%)
	public const int Handbrake		=  3;		// Handbrake position in ratio				10000
	public const int Clutch			=  4;		// Clutch pedal position in ratio			10000
	public const int ManualGear		=  5;		// Stick position for manual gears: -1 (R), 0 (N), 1, 2, 3... Might be used together with AutomaticGear in Manual (M) mode.
	public const int AutomaticGear	=  6;		// Stick position for automatic transmission: 0, 1, 2, 3, 4, 5 = M, P, R, N, D, L
	public const int GearShift		=  7;		// Incremental value for gear shifting: +1, -1. The vehicle sets this value back to 0 when acknowledged.
	public const int Retarder		=  8;		// Stick position for the retarder brake: 0 (disabled), 1, 2, 3...
	public const int Key			=  9;		// Ignition key position. -1 = off, 0 = acc-on, 1 = start.

	public const int Max			= 10;
	}


// Race input channel (write only)

public struct RaceInputData				// PID			DESCRIPTION															RESOLUTION		EXAMPLE
	{
	public const int Drs				=  0;		// DRS button
	public const int Push2Pass			=  1;		// Push to pass button
	public const int Flash				=  2;		// Flash headlights button
	public const int TcPreset 			=  3;		// Traction control preset. Zero should be interpreted as "not set" or "default".
	public const int TcOverride			=  4;		// Traction control override button (disable)
	public const int BrakeBias			=  5;		// Brake bias. Zero should be interpreted as "not set" or "default".		100			5800 (= 58% front, 42% rear)
	public const int AbsPreset			=  6;		// ABS preset. Zero should be interpreted as "not set" or "default".
	public const int EngineMap			=  7;		// Engine map preset. Zero should be interpreted as "not set" or "default".
	public const int DiffPreset			=  8;		// Differential preset. Zero should be interpreted as "not set" or "default".
	public const int RegenBraking		=  9;		// Regenerative braking preset. Zero should be interpreted as "not set" or "default".
	public const int TcPresetChange		= 10;		// Incremental value for traction control: +1, -1. Vehicle sets this value back to zero when acknowledged.
	public const int AbsPresetChange	= 11;		// Incremental value for ABS preset: +1, -1. Vehicle sets this value back to zero when acknowledged.
	public const int BrakeBiasChange	= 12;		// Incremental value for brake bias											100			-250 (= move 2.5% from front to rear)
	public const int EngineMapChange    = 13;		// Incremental value for engine map: +1, -1. Vehicle sets this value back to zero when acknowledged.
	public const int DiffPresetChange   = 14;		// Incremental value for differential preset: +1, -1. Vehicle sets this value back to zero when acknowledged.
	public const int RegenBrakingChange	= 15;		// Incremental value for regenerative braking: +1, -1. Vehicle sets this value back to zero when acknowledged.

	public const int Max				= 16;
	}


// Vehicle channel (read only)

public struct VehicleData			// PID			DESCRIPTION								RESOLUTION		EXAMPLE
	{
	public const int Speed			=  0;		// Vehicle's longitudinal velocity in m/s		1000		14500 (= 14.5 m/s)
	public const int EngineRpm		=  1;		// Engine RPMs									1000		1200000 (= 1200 rpm)
	public const int EngineStalled	=  2;		// Is the engine on but stalled? 0 = no, 1 = yes
	public const int EngineWorking	=  3;		// Is the engine up and running? 0 = no, 1 = yes
	public const int EngineStarting =  4;		// Is the engine starting as per the ignition input? 0 = no, 1 = yes
	public const int EngineLimiter	=  5;		// Is the rpm limiter cutting engine power? 0 = no, 1 = yes
	public const int EngineLoad		=  6;		// How much load is demanded in ratio			1000		200 (= 0.2 = 20%)
	public const int EngineTorque	=  7;		// Torque at the engine crankshaft in Nm		1000		200000 (= 200 Nm)
	public const int EnginePower	=  8;		// Power developed by the engine in kW			1000		10000 (= 10 kW)
	public const int EngineFuelRate =  9;		// Instant fuel consumption in g/s				1000		20230 (= 20.23 g/s)
	public const int ClutchTorque	= 10;		// Torque at the output of the clutch in Nm		1000		150000 (= 150 Nm)
	public const int ClutchLock		= 11;		// Lock ratio of the clutch						1000		800 (= 0.8 = 80%)
	public const int GearboxGear 	= 12;		// Engaged gear. Negative = reverse, 0 = Neutral (or Park), Positive = forward.
	public const int GearboxMode 	= 13;		// Gearbox working mode. 0, 1, 2, 3, 4, 5 = M, P, R, N, D, L
	public const int GearboxShifting= 14;		// Is the gearbox in the middle of a gear shift? 0 = no, 1 = yes
	public const int RetarderTorque = 15;		// Torque injected by the retarder in Nm		1000		2000000 (= 2000 Nm)
	public const int TransmissionRpm= 16;		// Rpms at the output of the gearbox			1000		100000 (= 100 rpm)
	public const int AbsEngaged 	= 17;		// Is the ABS being engaged in any wheel? 0 = no, 1 = yes
	public const int TcsEngaged 	= 18;		// Is the TCS limiting the engine throttle? 0 = no, 1 = yes
	public const int EscEngaged 	= 19;		// Is the ESC applying brakes for keeping stability? 0 = no, 1 = yes
	public const int AsrEngaged 	= 20;		// Is the ASR applying brakes for reducing wheel slip? 0 = no, 1 = yes
	public const int FuelConsumption= 21;		// Overall vehicle fuel consumption in l/100km	1000		20230 (= 20.23 l/100km)

	public const int AidedSteer		= 22;		// Steering applied after steering aids			10000       -10000 (full left), 0 (center), +10000 (full right)
	public const int BrakePressure  = 24;		// Pressure in the brakes circuit				1000		20000 (= 20 bar)
	public const int ThrottleSignal = 23;		// Final throttle signal applied to the engine	10000		5000 (= 0.5 = 50%)
	public const int ClutchSignal   = 25;		// Final clutch signal applied to the clutch	10000		5000 (= 0.5 = 50%)

	public const int Max			= 26;
	}


// Settings channel (read / write)

public struct SettingsData					// PID			DESCRIPTION							RESOLUTION		EXAMPLE
	{
	public const int DifferentialLock		=  0;		// Override the lock setting at the differential. 0 = no override. 1 = force locked. 2 = force open
	public const int DrivelineLock			=  1;		// Override the lock setting at the driveline. 0 = no override. 1 = force locked. 2 = force unlocked/open
	public const int AutoShiftOverride		=  2;		// Auto-shift override setting. 0 = no override. 1 = force auto shift. 2 = force manual shift.
	public const int AbsOverride			=  3;		// ABS override setting. 0 = no override. 1 = force enabled. 2 = force disabled.
	public const int EscOverride			=  4;		// ESC override setting. 0 = no override. 1 = force enabled. 2 = force disabled.
	public const int TcsOverride			=  5;		// TCS override setting. 0 = no override. 1 = force enabled. 2 = force disabled.
	public const int AsrOverride			=  6;		// ASR override setting. 0 = no override. 1 = force enabled. 2 = force disabled.
	public const int SteeringAidsOverride	=  7;		// Steering aids override setting. 0 = no override. 2 = force all steering aids disabled.

	public const int Max					= 10;
	}


// Data Bus implementation:
// A simple array (channels) of arrays of integers (values)


public class DataBus
	{
	int[][] m_data;


	public DataBus ()
		{
		// Create bus channels

		m_data = new int[Channel.Count][];
		m_data[Channel.Input] = new int[InputData.Max];
		m_data[Channel.RaceInput] = new int[RaceInputData.Max];
		m_data[Channel.Vehicle] = new int[VehicleData.Max];
		m_data[Channel.Settings] = new int[SettingsData.Max];
		m_data[Channel.Custom] = new int[100];
		}


	// Get a single value from a channel

	public int Get (int idChannel, int idValue)
		{
		return m_data[idChannel][idValue];
		}


	// Set a single value in a channel

	public void Set (int idChannel, int idValue, int value)
		{
		m_data[idChannel][idValue] = value;
		}


	// Direct access to a single channel

	public int[] Get (int idChannel)
		{
		return m_data[idChannel];
		}


	// Raw access to the entire bus: bus[channel][value]

	public int[][] bus => m_data;
	}

}

#endif
