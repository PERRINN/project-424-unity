using Unity.Barracuda;

namespace Perrinn424.AISpeedEstimatorSystem
{
    public class AISpeedEstimator
    {
        private readonly Model runtimeModel;
        private readonly IWorker worker;
        private float[] values;

        private float evaluateSpeed;
        public float EstimatedSpeed { get; private set; }

        public AISpeedEstimator(NNModel modelAsset)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = WorkerFactory.CreateWorker(runtimeModel);
            values = new float[AISpeedEstimatorInput.count];
        }

        public float Estimate(ref AISpeedEstimatorInput input)
        {
            UpdateValues(ref input);
            Tensor tensorInput = new Tensor(1, 1, 1, 10, values);
            worker.Execute(tensorInput);
            Tensor O = worker.PeekOutput();
            evaluateSpeed = O[0, 0, 0, 0]; //output. The model is in kmh
            EstimatedSpeed = evaluateSpeed / 3.6f; //transform to m/s
            tensorInput.Dispose();

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
