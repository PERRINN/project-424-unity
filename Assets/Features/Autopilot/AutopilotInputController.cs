using UnityEngine;
using VehiclePhysics;
using VehiclePhysics.InputManagement;


namespace Perrinn424.AutopilotSystem
{
    [RequireComponent(typeof(BaseAutopilot))]
    public class AutopilotInputController : VehicleBehaviour
    {
        [SerializeField]
        private BaseAutopilot autopilot;


        void OnEnable ()
        {
            InputManager.instance.runtime.disableForceFeedback = autopilot.IsOn;
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                autopilot.ToggleStatus();
                InputManager.instance.runtime.disableForceFeedback = autopilot.IsOn;
            }
        }


        private void Reset()
        {
            if (autopilot == null)
            {
                autopilot = this.GetComponent<BaseAutopilot>();
            }
        }
    }
}
