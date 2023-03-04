using UnityEngine;

public class Battery
{
    public class Settings
    {
        public float Efficiency = 0.85f;
        public float RadMassFlowAt50MeterPerSeconds = 2.1f; // kg/s
        public float CarSpeedToRadMassFlowPower = 1.17f;
        public float AvNormalisedHeatDissipation = 1692f; //W/degC
        public float AmbientTemperature = 15f; //degC
        public float ModuleToFluidExchangeEfficiency = 0.68f;
        public float CellOnlyModuleHeatCapacity = 13675f;
        public float NumberOfModules = 10f;
    }

    public Settings settings = new Settings();

    public float TotalHeat { get; private set; } //W
    public float AirMassFlow { get; private set; } //kg/s
    public float HeatDissipation { get; private set; } //W/degC
    public float HeatDissipated { get; private set; } //J
    public float TemperatureModule { get; private set; } //degC
    public float QInteral { get; private set; } //J


    public void InitModel(float dt, float speed, float power)
    {
        TotalHeat = (1f - settings.Efficiency) * Mathf.Abs(power) * 1000f;
        AirMassFlow = settings.RadMassFlowAt50MeterPerSeconds * Mathf.Pow(speed, settings.CarSpeedToRadMassFlowPower) / Mathf.Pow(50, settings.CarSpeedToRadMassFlowPower);
        HeatDissipation = AirMassFlow * settings.AvNormalisedHeatDissipation / 2.5f;
        HeatDissipated = 0f;
        TemperatureModule = 15f;
        QInteral = TotalHeat * dt + HeatDissipated;
    }

    public void UpdateModel(float dt, float speed, float power)
    {
        TotalHeat = (1f - settings.Efficiency) * Mathf.Abs(power) * 1000f;
        AirMassFlow = settings.RadMassFlowAt50MeterPerSeconds * Mathf.Pow(speed, settings.CarSpeedToRadMassFlowPower) / Mathf.Pow(50, settings.CarSpeedToRadMassFlowPower);
        HeatDissipation = AirMassFlow * settings.AvNormalisedHeatDissipation / 2.5f;
        HeatDissipated = -HeatDissipation*dt*(TemperatureModule - settings.AmbientTemperature)*settings.ModuleToFluidExchangeEfficiency;
        TemperatureModule = TemperatureModule + QInteral/(settings.CellOnlyModuleHeatCapacity*settings.NumberOfModules);
        QInteral = TotalHeat * dt + HeatDissipated;

    }
}
