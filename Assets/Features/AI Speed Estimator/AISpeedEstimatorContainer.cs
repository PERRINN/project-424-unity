using Perrinn424.TelemetryLapSystem;
using Unity.Barracuda;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.AISpeedEstimatorSystem
{
    public class AISpeedEstimatorContainer : VehicleBehaviour
    {
        [SerializeField]
        private Channels channels;

        public NNModel modelAsset;
        //public Model runtimeModel;
        //public IWorker worker;

        //public float[] inputValues;

        public float speed;
        public float evaluateSpeed;
        public float Error { get; private set; }

        public float EstimatedSpeed { get; private set; }

        private AISpeedEstimator aiSpeedEstimator;
        private AISpeedEstimatorInput input;

        //void Start()
        //{
        //    runtimeModel = ModelLoader.Load(modelAsset);
        //    worker = WorkerFactory.CreateWorker(runtimeModel);
        //}

        public override void OnEnableVehicle()
        {
            aiSpeedEstimator = new AISpeedEstimator(modelAsset);

            channels.Reset(vehicle);

            //inputValues = new float[10];
        }

        private void Update()
        {
            //    for (int i = 0; i < channels.Length; i++)
            //    {
            //        inputValues[i] = channels.GetValue(i);
            //    }

            input.throttle = channels.GetValue(0);
            input.brake = channels.GetValue(1);
            input.accelerationLateral = channels.GetValue(2);
            input.accelerationLongitudinal = channels.GetValue(3);
            input.accelerationVertical = channels.GetValue(4);
            input.nWheelFL = channels.GetValue(5);
            input.nWheelFR = channels.GetValue(6);
            input.nWheelRL = channels.GetValue(7);
            input.nWheelRR = channels.GetValue(8);
            input.steeringAngle = channels.GetValue(9);

            EstimatedSpeed = aiSpeedEstimator.Estimate(ref input);
            speed = vehicle.speed;
            Error = Mathf.Abs(speed - EstimatedSpeed);


            //Tensor input = new Tensor(1, 1, 1, 10, inputValues);
            //worker.Execute(input);
            //Tensor O = worker.PeekOutput();
            //evaluateSpeed = O[0, 0, 0, 0];
            //EstimatedSpeed = evaluateSpeed / 3.6f;
            //speed = vehicle.speed;
            //Error = Mathf.Abs(speed - EstimatedSpeed);
            //input.Dispose();
        }
    } 
}
