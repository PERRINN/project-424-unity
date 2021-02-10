using UnityEngine;

namespace Perrinn424.UI
{
    public class UserInterfaceController : MonoBehaviour
    {
        [SerializeField]
        private Behaviour[] fullUI;
        [SerializeField]
        private Behaviour[] fullScreen;

        private bool isFullUI = true;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isFullUI = !isFullUI;
                SetEnable(fullUI, isFullUI);
                SetEnable(fullScreen, !isFullUI);
            }
        }

        private void SetEnable(Behaviour[] componentArray, bool isEnable)
        {
            foreach (Behaviour component in componentArray)
            {
                component.enabled = isEnable;
            }
        }
    } 
}
