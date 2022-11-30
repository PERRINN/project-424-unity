using UnityEngine;
using Cinemachine;

[SaveDuringPlay, ExecuteAlways]
[AddComponentMenu("")] // Hide in menu
public class ClosestCameraEvaluator : CinemachineExtension
{
    [Range(0,1)]
    public float qualityVsDistance = 0.5f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            if (state.HasLookAt)
            {
                float distance = Vector3.Distance(state.ReferenceLookAt, state.FinalPosition);
                state.ShotQuality = Mathf.Lerp(state.ShotQuality / distance, 1f / distance, qualityVsDistance);
            }
        }
    }
}