using System.Collections;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424
{
    public class AccelerationTest : VehicleBehaviour
    {
        public float maxSpeed = 100f;

        private float startingTime;
        public bool TestFinished { get; private set; }
        public float ElapsedTime { get; private set; }
        public float TraveledDistance => traveledDistance;

        private float traveledDistance;



        private IEnumerator Start()
        {
            startingTime = Time.time;
            SetInput(InputData.AutomaticGear, 4);
            SetInput(InputData.Throttle, 10000);
            yield return new WaitForSpeed(vehicle, maxSpeed);
            TestFinished = true;
            SetInput(InputData.Throttle, 0);
            SetInput(InputData.Brake, 10000);
            yield return new WaitUntil(() => vehicle.speed < 0.001f);

            Debug.Log($"Elapsed Time: {ElapsedTime} s to reach {maxSpeed} m/s. Traveled distance: {traveledDistance} m");

        }

        public override void FixedUpdateVehicle()
        {
            if (!TestFinished)
            {
                traveledDistance += vehicle.speed * Time.deltaTime;
                ElapsedTime = Time.time - startingTime;
            }
        }

        protected void SetInput(int idValue, int value)
        {
            vehicle.data.Set(Channel.Input, idValue, value);
        }

        private void OnGUI()
        {
            GUILayout.Label($"Traveled Distance: {TraveledDistance: 0.00} [m]");

            float t;
            if (TestFinished)
            {
                t = ElapsedTime;
            }
            else
            {
                t = Time.time - startingTime;
            }

            float throttle = vehicle.data.Get(Channel.Input, InputData.Throttle)/10000.0f;
            float brake = vehicle.data.Get(Channel.Input, InputData.Brake) /10000.0f;

            GUILayout.Label($"Elapsed time: {t:0.00} [s]");
            GUILayout.Label($"Speed: {vehicle.speed:0.00} [m/s]");
            GUILayout.Label($"Throttle: {throttle:P2} [%]");
            GUILayout.Label($"Brake: {brake:P2} [%]");
        }
    } 
}
