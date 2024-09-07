
using Unity.Sentis;

namespace Perrinn424.AISpeedEstimatorSystem
{
    public class AISpeedEstimator
    {
        private readonly Model runtimeModel;
        private readonly IWorker worker;
        private float[] values;

        private float evaluateSpeed;
        public float EstimatedSpeed { get; private set; }

        public AISpeedEstimator(ModelAsset modelAsset)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = WorkerFactory.CreateWorker(BackendType.CPU, runtimeModel);
            values = new float[AISpeedEstimatorInput.count];
        }

        /// <summary>
        /// Estimate the speed in m/s
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float Estimate(ref AISpeedEstimatorInput input)
        {
            UpdateValues(ref input);
            // Create a tensor for input data
            TensorFloat tensorInput = new TensorFloat(new TensorShape(1, 10), values);  // Ensure TensorShape matches your input dimensions

            // Execute the model with the input tensor
            worker.Execute(tensorInput);

            // Get the output tensor
            //TensorFloat tensorOutput = worker.PeekOutput() as TensorFloat;

            TensorFloat tensorOutput = worker.PeekOutput() as TensorFloat;
            //UnityEngine.Debug.Log(tensorOutput.ToReadOnlyArray());

            // Read the first value in the output (assuming it’s the speed)
            evaluateSpeed = tensorOutput.ToReadOnlyArray()[0];  // First value of the output tensor
            EstimatedSpeed = evaluateSpeed / 3.6f;  // Convert from km/h to m/s

            tensorInput.Dispose();
            tensorOutput.Dispose();

            return EstimatedSpeed;
        }

        private void UpdateValues(ref AISpeedEstimatorInput input) 
        {
            values[0] = input.throttle;
            values[1] = input.brake;
            values[2] = input.accelerationLateral;
            values[3] = input.accelerationLongitudinal;
            values[4] = input.accelerationVertical;
            values[5] = input.nWheelFL;
            values[6] = input.nWheelFR;
            values[7] = input.nWheelRL;
            values[8] = input.nWheelRR;
            values[9] = input.steeringAngle;
    }
    } 
}
