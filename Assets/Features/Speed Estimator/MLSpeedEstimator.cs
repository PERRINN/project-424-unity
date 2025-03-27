
using System;
using Unity.Sentis;

namespace Perrinn424.SpeedEstimatorSystem
{
    public class MLSpeedEstimator : IDisposable
    {
        private readonly Model runtimeModel;
        private readonly IWorker engine;
        private readonly TensorFloat tensorInput;

        private float evaluateSpeed;
        public float EstimatedSpeed { get; private set; }

        public MLSpeedEstimator(ModelAsset modelAsset)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            engine = WorkerFactory.CreateWorker(BackendType.CPU, runtimeModel);
            tensorInput = TensorFloat.Zeros(new TensorShape(1, MLSpeedEstimatorInput.count));
        }

        /// <summary>
        /// Estimate the speed in m/s
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float Estimate(ref MLSpeedEstimatorInput input)
        {
            UpdateValues(ref input);

            engine.Execute(tensorInput);

            TensorFloat tensorOutput = engine.PeekOutput() as TensorFloat;

            evaluateSpeed = tensorOutput[0];  // First value of the output tensor
            EstimatedSpeed = evaluateSpeed / 3.6f;  // Convert from km/h to m/s

            tensorOutput.Dispose();

            return EstimatedSpeed;
        }

        private void UpdateValues(ref MLSpeedEstimatorInput input) 
        {
            tensorInput[0] = input.throttle;
            tensorInput[1] = input.brake;
            tensorInput[2] = input.accelerationLateral;
            tensorInput[3] = input.accelerationLongitudinal;
            tensorInput[4] = input.accelerationVertical;
            tensorInput[5] = input.nWheelFL;
            tensorInput[6] = input.nWheelFR;
            tensorInput[7] = input.nWheelRL;
            tensorInput[8] = input.nWheelRR;
            tensorInput[9] = input.steeringAngle;
    }

        public void Dispose()
        {
            tensorInput.Dispose();
            engine.Dispose();
        }
    } 
}
