using Perrinn424.TelemetryLapSystem;
using Unity.Barracuda;
using UnityEngine;
using VehiclePhysics;

public class AISpeedEstimator : VehicleBehaviour
{
    [SerializeField]
    private Channels channels;

    public NNModel modelAsset;
    public Model runtimeModel;
    public IWorker worker;

    public float[] inputValues;

    public float speed;
    public float evaluateSpeed;
    public float Error { get; private set; }

    public float EstimatedSpeed { get; private set; }

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(runtimeModel);
    }

    public override void OnEnableVehicle()
    {
        channels.Reset(vehicle);

        inputValues = new float[10];
    }

    private void Update()
    {
        for (int i = 0; i < channels.Length; i++)
        {
            inputValues[i] = channels.GetValue(i);
        }

        Tensor input = new Tensor(1, 1, 1, 10, inputValues);
        worker.Execute(input);
        Tensor O = worker.PeekOutput();
        evaluateSpeed = O[0, 0, 0, 0];
        EstimatedSpeed = evaluateSpeed/3.6f;
        speed = vehicle.speed;
        Error = Mathf.Abs(speed - EstimatedSpeed);
        input.Dispose();
    }
}
