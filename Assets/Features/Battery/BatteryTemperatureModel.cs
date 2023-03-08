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
            public float efficiency = 0.85f;
            public float radMassFlowAt50MeterPerSeconds = 2.1f; // kg/s
            public float carSpeedToRadMassFlowPower = 1.17f;
            public float avNormalisedHeatDissipation = 1692f; //W/degC
            public float ambientTemperature = 15f; //degC
            public float moduleToFluidExchangeEfficiency = 0.68f;
            public float cellOnlyModuleHeatCapacity = 13675f;
            public float numberOfModules = 10f;
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
            TotalHeat = CaculateTotalHeat(power, settings.efficiency);
            AirMassFlow = CalculateAirMassFlow(speed, settings.radMassFlowAt50MeterPerSeconds, settings.carSpeedToRadMassFlowPower);
            HeatDissipation = CalculateHeatDissipation(AirMassFlow, settings.avNormalisedHeatDissipation);
            HeatDissipated = 0f;
            TemperatureModule = settings.ambientTemperature;
            HeatInternal = CalculateHeatInternal(TotalHeat, HeatDissipated, dt);
        }

        public void Reset(float dt, float speed, float power)
        {
            InitModel(dt, speed, power);
        }

        public void UpdateModel(float dt, float speed, float power)
        {
            TotalHeat = CaculateTotalHeat(power, settings.efficiency);
            AirMassFlow = CalculateAirMassFlow(speed, settings.radMassFlowAt50MeterPerSeconds, settings.carSpeedToRadMassFlowPower);
            HeatDissipation = CalculateHeatDissipation(AirMassFlow, settings.avNormalisedHeatDissipation);
            HeatDissipated = CalculateHeatDissipated(HeatDissipation, TemperatureModule, settings.ambientTemperature, settings.moduleToFluidExchangeEfficiency, dt);
            TemperatureModule = CalculateTemperatureModule(TemperatureModule, HeatInternal, settings.cellOnlyModuleHeatCapacity, settings.numberOfModules);
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
