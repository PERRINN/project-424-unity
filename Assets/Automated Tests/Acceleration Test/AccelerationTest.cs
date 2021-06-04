using System.Collections;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class AccelerationTest : VehicleBehaviour
    {
        public float distance = 1100.0f;

        private WaitForDistance waitForDistance;
        private float startingTime;
        public bool TestFinished { get; private set; }
        public float ElapsedTime { get; private set; }
        public float TraveledDistance => waitForDistance.TraveledDistance;

        public override void OnEnableVehicle()
        {
            waitForDistance = new WaitForDistance(vehicle, distance);
        }

        private IEnumerator Start()
        {
            startingTime = Time.time;
            SetInput(InputData.AutomaticGear, 4);
            SetInput(InputData.Throttle, 10000);
            yield return waitForDistance;
            SetInput(InputData.Throttle, 0);
            SetInput(InputData.Brake, 10000);
            yield return new WaitUntil(() => vehicle.speed < 0.001f);
            ElapsedTime = Time.time - startingTime;

            Debug.Log($"Elapsed Time: {ElapsedTime}");

            TestFinished = true;
        }

        protected void SetInput(int idValue, int value)
        {
            vehicle.data.Set(Channel.Input, idValue, value);
        }

        private void OnGUI()
        {
            GUILayout.Label($"Traveled Distance: {TraveledDistance: 0.00}");

            float t;
            if (TestFinished)
            {
                t = ElapsedTime;
            }
            else
            {
                t = Time.time - startingTime;
            }

            GUILayout.Label($"Elapsed time: {t:0.00}");

        }
    } 
}
