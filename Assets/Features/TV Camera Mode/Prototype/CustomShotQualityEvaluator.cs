using UnityEngine;
using Cinemachine;

[SaveDuringPlay, ExecuteAlways]
[AddComponentMenu("")] // Hide in menu
public class CustomShotQualityEvaluator : CinemachineExtension
{
    public float NearLimit;
    public float FarLimit;
    public float OptimalDistance;
    public float MaxQualityBoost;

    void OnValidate()
    {
        NearLimit = Mathf.Max(0.1f, NearLimit);
        FarLimit = Mathf.Max(NearLimit, FarLimit);
        OptimalDistance = Mathf.Clamp(OptimalDistance, NearLimit, FarLimit);
    }

    void Reset()
    {
        NearLimit = 5;
        FarLimit = 30;
        OptimalDistance = 10;
        MaxQualityBoost = 0.2f;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            float nearnessBoost = 0;
            if (OptimalDistance > 0 && state.HasLookAt)
            {
                float distance = Vector3.Magnitude(state.ReferenceLookAt - state.FinalPosition);
                if (distance <= OptimalDistance)
                {
                    if (distance >= NearLimit)
                        nearnessBoost = MaxQualityBoost * (distance - NearLimit)
                            / (OptimalDistance - NearLimit);
                }
                else
                {
                    distance -= OptimalDistance;
                    if (distance < FarLimit)
                        nearnessBoost = MaxQualityBoost * (1f - (distance / FarLimit));
                }
                state.ShotQuality *= (1f + nearnessBoost);
            }
        }
    }
}