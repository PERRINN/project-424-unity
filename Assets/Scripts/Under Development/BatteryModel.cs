using VehiclePhysics;
using VehiclePhysics.UI;

public class BatteryModel : VehicleBehaviour
{
    public static float batteryCapacity;
    public static float batterySOC;
    public static float frontPower;
    public static float rearPower;
    public static float powerTotal;


    float batteryTemprature;
    float batteryDOD;
    float batteryVoltage;


    private void Start()
    {
        batteryCapacity = 55;
        batterySOC = batteryCapacity;
        batteryDOD = 0;
    }


    public override void FixedUpdateVehicle()
    {
        int[] custom = vehicle.data.Get(Channel.Custom);

        frontPower = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;
        rearPower = custom[Perrinn424Data.RearMguBase + Perrinn424Data.ElectricalPower] / 1000.0f;

        powerTotal = frontPower + rearPower;

        BatteryCharge();

        batterySOC = (batteryCapacity / 55) * 100;
        batteryDOD = 100 - batterySOC;
    }

    public override void UpdateVehicle()
    {
        SteeringScreen.batSOC = batterySOC;
        SteeringScreen.batCapacity = batteryCapacity;
    }

    void BatteryCharge()
    {
        if (powerTotal > 0)
        {
            batteryCapacity -= ((powerTotal / 60) / 60) /500;
        }
        else if (powerTotal < 0)
        {
            batteryCapacity -= ((powerTotal / 60) / 60) / 500;
        }

        if (batteryCapacity < 0)
        {
            batteryCapacity = 0;
        }
    }
}
