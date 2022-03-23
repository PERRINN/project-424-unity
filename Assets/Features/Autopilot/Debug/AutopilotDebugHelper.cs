using Perrinn424.AutopilotSystem;
using UnityEngine;
using VehiclePhysics.Timing;

public class AutopilotDebugHelper : MonoBehaviour, IPIDInfo
{
    public AutopilotDeprecated autopilot;
    public Autopilot autopilotExperimental;

    public Behaviour[] autopilotInLap;

    public LapTimer lapTimer;
    public int lapCount;
    private IPIDInfo selectedAutopilot;

    private Behaviour defaultAutopilot;
    private IPIDInfo SelectedAutopilot
    {
        get
        {
            return selectedAutopilot ?? autopilot;
        }
    }

    private void OnEnable()
    {
        autopilot.enabled = false;
        autopilotExperimental.enabled = false;
        lapTimer.onBeginLap += LapBeginEventHandler;
        SelectAutopilot();
    }

    private void OnDisable()
    {
        lapTimer.onBeginLap -= LapBeginEventHandler;
    }

    private void LapBeginEventHandler()
    {
        SelectAutopilot();
        lapCount++;
    }

    private void SelectAutopilot()
    {
        foreach (var component in autopilotInLap)
        {
            component.enabled = false;
        }

        if (lapCount < autopilotInLap.Length)
        {
            autopilotInLap[lapCount].enabled = true;
            selectedAutopilot = (IPIDInfo)autopilotInLap[lapCount];
        }
        else
        {
            autopilot.enabled = true;
            selectedAutopilot = autopilot;
        }

    }

    public float Error => SelectedAutopilot.Error;

    public float P => SelectedAutopilot.P;

    public float I => SelectedAutopilot.I;

    public float D => SelectedAutopilot.D;

    public float PID => SelectedAutopilot.PID;

    public float MaxForceP => SelectedAutopilot.MaxForceP;

    public float MaxForceD => SelectedAutopilot.MaxForceD;
}
