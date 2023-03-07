using System;
using UnityEngine;

namespace Perrinn424
{
    [Serializable]
    public class BatteryTemperatureModel
    {
        [Serializable]
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
        public float HeatInternal { get; private set; } //J


        public void InitModel(float dt, float speed, float power)
        {
            TotalHeat = CaculateTotalHeat(power, settings.Efficiency);
            AirMassFlow = CalculateAirMassFlow(speed, settings.RadMassFlowAt50MeterPerSeconds, settings.CarSpeedToRadMassFlowPower);
            HeatDissipation = CalculateHeatDissipation(AirMassFlow, settings.AvNormalisedHeatDissipation);
            HeatDissipated = 0f;
            TemperatureModule = settings.AmbientTemperature;
            HeatInternal = CalculateHeatInternal(TotalHeat, HeatDissipated, dt);
        }

        public void Reset(float dt, float speed, float power)
        {
            InitModel(dt, speed, power);
        }

        public void UpdateModel(float dt, float speed, float power)
        {
            TotalHeat = CaculateTotalHeat(power, settings.Efficiency);
            AirMassFlow = CalculateAirMassFlow(speed, settings.RadMassFlowAt50MeterPerSeconds, settings.CarSpeedToRadMassFlowPower);
            HeatDissipation = CalculateHeatDissipation(AirMassFlow, settings.AvNormalisedHeatDissipation);
            HeatDissipated = CalculateHeatDissipated(HeatDissipation, TemperatureModule, settings.AmbientTemperature, settings.ModuleToFluidExchangeEfficiency, dt);
            TemperatureModule = CalculateTemperatureModule(TemperatureModule, HeatInternal, settings.CellOnlyModuleHeatCapacity, settings.NumberOfModules);
            HeatInternal = CalculateHeatInternal(TotalHeat, HeatDissipated, dt);
        }

        private static float CaculateTotalHeat(float power, float efficiency)
        {
            return (1f - efficiency) * Mathf.Abs(power) * 1000f;
        }

        private static float CalculateAirMassFlow(float speed, float radMassFlowAt50MeterPerSeconds, float carSpeedToRadMassFlowPower)
        {
            return radMassFlowAt50MeterPerSeconds * Mathf.Pow(speed, carSpeedToRadMassFlowPower) / Mathf.Pow(50f, carSpeedToRadMassFlowPower);
        }

        private static float CalculateHeatDissipation(float airMassFlow, float avNormalisedHeatDissipation)
        {
            return airMassFlow * avNormalisedHeatDissipation / 2.5f;
        }

        private static float CalculateHeatDissipated(float heatDissipation, float temperatureModule, float ambientTemperature, float moduleToFluidExchangeEfficiency, float dt)
        {
            return -heatDissipation * dt * (temperatureModule - ambientTemperature) * moduleToFluidExchangeEfficiency;
        }

        private static float CalculateTemperatureModule(float temperatureModulePrevious, float qInternalPrevious, float cellOnlyModuleHeatCapacity, float numberOfModules)
        {
            return temperatureModulePrevious + qInternalPrevious / (cellOnlyModuleHeatCapacity * numberOfModules);
        }

        private static float CalculateHeatInternal(float totalHeat, float heatDissipated, float dt)
        {
            return heatDissipated + totalHeat * dt;
        }
    } 
}
